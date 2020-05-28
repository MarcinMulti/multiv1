using GH_IO.Serialization;
using Rhino.FileIO;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multiconsult_V001.Classes;
using Rhino.Geometry.Intersect;

namespace Multiconsult_V001.Methods
{
    class Geometry
    {
        public static int[] findBottomAndTopBrepFace(Brep brep)
        {
            //variables
            Dictionary<int, double> dicZ = new Dictionary<int, double>();
            int iz = 0;

            //methods
            foreach (var f in brep.Faces)
            {
                //find the center point of each brep face
                Point3d center = AreaMassProperties.Compute(f.ToBrep()).Centroid;
                dicZ.Add(iz++, center.Z);
            }

            //sort results
            double minZ = dicZ.OrderBy(z => z.Value).First().Value;
            int iminZ = dicZ.OrderBy(z => z.Value).First().Key;
            double maxZ = dicZ.OrderBy(z => z.Value).Last().Value;
            int imaxZ = dicZ.OrderBy(z => z.Value).Last().Key;

            //outputs
            int[] bt = new int[2];
            bt[0] = iminZ;  //id of the lowest brepface in the brep
            bt[1] = imaxZ;  //id of the top brepface in the brep

            return bt;
        }
        public static Point3d[] findBottomAndTopCenterPoints(Brep brep)
        {
            //variables
            var ids = Methods.Geometry.findBottomAndTopBrepFace(brep);

            //sort results
            var botPt = AreaMassProperties.Compute(brep.Faces[ids[0]].ToBrep()).Centroid;
            var topPt = AreaMassProperties.Compute(brep.Faces[ids[1]].ToBrep()).Centroid;

            //outputs
            Point3d[] pts = new Point3d[2];
            pts[0] = botPt;  //id of the lowest brepface in the brep
            pts[1] = topPt;  //id of the top brepface in the brep

            return pts;
        }


        public static Column_Section findColumnSection(Brep brep)
        {
            //variables
            var ids = Methods.Geometry.findBottomAndTopBrepFace(brep);
            var idcrvs = brep.Faces[ids[0]].AdjacentEdges();
            Column_Section cs1 = new Column_Section();
            List<Curve> sectionCrvs = new List<Curve>();
            double d1 = 0;
            double d2 = 0;
            Vector3d v1 = new Vector3d();
            Vector3d v2 = new Vector3d();
            Point3d center = findBottomAndTopCenterPoints(brep)[0];

            bool isCircle = false;
            //get the curves from brep face
            foreach (var ic in idcrvs)
            {
                sectionCrvs.Add(brep.Edges[ic].ToNurbsCurve());
                if (brep.Edges[ic].IsArc())
                {
                    isCircle = true;

                }
            }

            if (isCircle)
            {
                cs1.name = "circle section";
                cs1.type = 0;
                var arc1 = new Arc();
                brep.Edges[0].TryGetArc(out arc1);
                cs1.dim1 = Math.Round(arc1.Center.DistanceTo(arc1.EndPoint), 5); //radius
                cs1.dim2 = 2 * d1; //diameter
                cs1.area = Math.PI * cs1.dim1 * cs1.dim1;
                v1 = Point3d.Subtract(center, arc1.StartPoint);
                v2 = Point3d.Subtract(center, arc1.MidPoint);
            }
            else
            {
                var edge1 = new Line(brep.Edges[0].PointAtStart, brep.Edges[0].PointAtEnd);
                var edge2 = new Line(brep.Edges[1].PointAtStart, brep.Edges[1].PointAtEnd);
                d1 = edge1.Length;
                d2 = edge2.Length;
                v1 = Point3d.Subtract(center, edge1.PointAt(0.5));
                v2 = Point3d.Subtract(center, edge2.PointAt(0.5));

                if (d1 == d2)
                {
                    //square section
                    cs1.name = "square section";
                    cs1.type = 1;
                    cs1.dim1 = d1;
                    cs1.dim2 = d2;
                    cs1.area = cs1.dim1 * cs1.dim2;
                }
                else
                {
                    //rectangular section
                    cs1.name = "rectangle section";
                    cs1.type = 2;
                    cs1.dim1 = d1;
                    cs1.dim2 = d2;
                    cs1.area = cs1.dim1 * cs1.dim2;
                }
            }

            cs1.vec1 = v1;
            cs1.vec2 = v2;
            cs1.edges = sectionCrvs.ToArray();

            //outputs
            return cs1;
        }

        public static Floor_Section findFloorSection(Brep brep)
        {
            //get the distance between the lowest and heighest center point z coordinate
            double thick = Math.Round(findBottomAndTopCenterPoints(brep)[1].Z - findBottomAndTopCenterPoints(brep)[0].Z, 5);
            Floor_Section fs = new Floor_Section("plane floor", thick);

            return fs;
        }

        public static Plane[] findWallHorizontalPlanes(Brep brep)
        {
            //main param the array of the planes
            var pls = new Plane[3];

            var cpts = Methods.Geometry.findBottomAndTopCenterPoints(brep);  //find bottom and top central points

            //find central plane
            pls[0] = (new Plane(new Line(cpts[0], cpts[1]).PointAt(0.5), new Vector3d(0, 0, 1))); //XY plane in the middle of the wall height

            //find bottom and top planes
            pls[1] = (new Plane(cpts[0], new Vector3d(0, 0, 1)));
            pls[2] = (new Plane(cpts[1], new Vector3d(0, 0, 1)));

            //outputs
            return pls;
        }


        public static List<Curve> findWallBottomAndTop(Brep brep)
        {
            var crvs = new List<Curve>();
            var tcrvs = new List<Curve>(); //temporary variable

            var cpts = Methods.Geometry.findBottomAndTopCenterPoints(brep);  //find bottom and top central points
            var pls = Methods.Geometry.findWallHorizontalPlanes(brep);

            //intersect the brep with the plane to find master surface and boundaries
            Curve[] icrvs;
            Point3d[] ipts;
            var didItSect = Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, pls[0], 0.00001, out icrvs, out ipts);
            tcrvs = icrvs.ToList();

            //transformation from central plane to bottom tb and top tt
            var tb = Transform.PlaneToPlane(pls[0], pls[1]);
            var tt = Transform.PlaneToPlane(pls[0], pls[2]);

            //make closed polylines
            var bcs = Curve.JoinCurves(tcrvs);
            var tcs = Curve.JoinCurves(tcrvs);

            //transform to bottom
            foreach (var bc in bcs)
            {
                bc.Transform(tb);
            }

            //transform to top
            foreach (var tc in tcs)
            {
                tc.Transform(tt);
            }
            crvs.AddRange(Curve.JoinCurves(icrvs));
            crvs.AddRange(bcs);
            crvs.AddRange(tcs);

            return crvs;
        }

        public static Line[] findWallConstructionLines(Curve[] crvs, Point3d[] cpts)
        {
            //variable
            Line[] lines = new Line[3];
            List<Curve> tcvs = new List<Curve>(); //top boundaries

            //find the longest curve
            Dictionary<Line, double> dedges = new Dictionary<Line, double>();
            NurbsCurve ncrvs = crvs[0].ToNurbsCurve();

            for (int i = 0; i < ncrvs.Points.Count - 1; i++)
            {
                var l = new Line(ncrvs.Points[i].Location, ncrvs.Points[i + 1].Location);
                dedges.Add(l, l.Length);
            }
            var longest = dedges.OrderBy(x => x.Value).Last().Key;
            var vecWx = Point3d.Subtract(longest.From, longest.To);
            var vecWy = Point3d.Subtract(longest.From, longest.To);
            var vecToMiddle = Point3d.Subtract(longest.From, longest.PointAt(0.5));

            var rotate90 = Transform.Rotation(0.5 * Math.PI, longest.From);
            var moveToMiddle = Transform.Translation(vecToMiddle);
            vecWy.Transform(rotate90);
            vecWy.Transform(moveToMiddle);

            Plane tpl = new Plane(longest.PointAt(0.5), vecWy, new Vector3d(0, 0, 1));

            var ints = Rhino.Geometry.Intersect.Intersection.CurvePlane(ncrvs, tpl, 0.000001);

            List<Point3d> ipts = new List<Point3d>();
            foreach (var iints in ints)
            {
                ipts.Add(iints.PointA);
            }
            tcvs.Add(longest.ToNurbsCurve());
            tcvs.Add(new Line(longest.PointAt(0.5), vecWy).ToNurbsCurve());

            Line midleLine = new Line(ipts[1], ipts[0]);
            var moveToMiddleY = Transform.Translation((Point3d.Subtract(midleLine.PointAt(0.5), longest.PointAt(0.5))));
            longest.Transform(moveToMiddleY);

            Line hLine = new Line(
                new Point3d(midleLine.PointAt(0.5).X, midleLine.PointAt(0.5).Y, cpts[0].Z),
                new Point3d(midleLine.PointAt(0.5).X, midleLine.PointAt(0.5).Y, cpts[1].Z)
                );
            

            lines[0] = longest;
            lines[1] = midleLine;
            lines[2] = hLine;

            //outputs
            return lines;
        }


        public static Dictionary<int, Column> createColumnsFromFloorPlanes(Dictionary<int, Column> col, Dictionary<int, Floor> flo)
        {
            
            //methods
            Dictionary<int, Column> newColumns = new Dictionary<int, Column>();
            List<Plane> floorPlanes = new List<Plane>();
            int ic = 0; //counter for new columns

            //take all planes from surfaces
            foreach (var p in flo.Values)
            {
                floorPlanes.Add(p.plane);
            }
            //order them by zorigin
            var fp = floorPlanes.OrderBy(px => px.OriginZ).ToList();

            //loop over planes
            for (int ip = 0; ip < fp.Count - 1; ip++)
            {
                Plane bottomPlane = fp[ip];
                Plane topPlane = fp[ip + 1];

                foreach (var c in col.Values)
                {
                    double bottomParameter;
                    double topParameter;
                    Intersection.LinePlane(c.line, bottomPlane, out bottomParameter);
                    Intersection.LinePlane(c.line, topPlane, out topParameter);

                    double distFromBotPl = c.line.PointAt(bottomParameter).DistanceTo(c.line.PointAt(0));
                    double distFromTopPl = c.line.PointAt(topParameter).DistanceTo(c.line.PointAt(1));

                    Line nline = new Line(c.line.PointAt(bottomParameter), c.line.PointAt(topParameter)); //new line
                    bool addLine = false;
                    //check if the parameters are close to the line
                    if (bottomParameter >= 0 && bottomParameter <= 1)
                    {
                        if (topParameter >= 0 && topParameter <= 1) //bottom intersection is in the proper range
                        {
                            addLine = true;   //top intersection is in the proper range
                        }
                        else if (distFromTopPl <= 0.4 * nline.Length)
                        {
                            addLine = true;  //top intersection is in the proper range
                        }
                    }
                    else if (topParameter >= 0 && topParameter <= 1)
                    {
                        if (bottomParameter >= 0 && bottomParameter <= 1)   //top intersection is in the proper range
                        {
                            addLine = true;   //top intersection is in the proper range
                        }
                        else if (distFromBotPl <= 0.4 * nline.Length)
                        {
                            addLine = true;   //top intersection is in the proper range
                        }
                    }
                    else if (distFromBotPl <= 0.4 * nline.Length && distFromTopPl <= 0.4 * nline.Length)
                    {
                        addLine = true;  //top intersection is in the proper range
                    }
                    else
                    {
                        //do nothing
                    }

                    //create new column
                    if (addLine)
                    {

                        Column nC = new Column(ic, nline);
                        nC.section = c.section;
                        nC.material = c.material;
                        nC.name = c.name;

                        newColumns.Add(ic, nC);
                        ic++;
                    }

                }
            }

            return newColumns;
        }

        //method for splitting walls vertically
        public static Dictionary<int, Wall> createWallsFromFloorPlanes(Dictionary<int, Wall> wals, Dictionary<int, Floor> flos)
        {
            //method for walls
            Dictionary<int, Wall> newWalls = new Dictionary<int, Wall>();
            List<Plane> floorPlanes = new List<Plane>();
            int iw = 0; //counter for new columns

            //take all planes from surfaces
            foreach (var p in flos.Values)
            {
                floorPlanes.Add(p.plane);
            }
            //order them by zorigin
            var fp = floorPlanes.OrderBy(px => px.OriginZ).ToList();

            //loop over planes to make section according floors
            for (int ip = 0; ip < fp.Count - 1; ip++)
            {
                Plane bottomPlane = fp[ip];
                Plane topPlane = fp[ip + 1];
                Plane middlePlane = new Plane(new Line(bottomPlane.Origin, topPlane.Origin).PointAt(0.5), new Vector3d(0, 0, 1));
                foreach (var w in wals.Values)
                {
                    bool isIntersecting = false;
                    Curve[] sectionCurve;
                    Point3d[] sectionPts;
                    Intersection.BrepPlane(w.surface, middlePlane, 0.000001, out sectionCurve, out sectionPts);

                    //create new column
                    if (sectionCurve != null)
                    {
                        Line hLine = new Line(
                            sectionCurve[0].PointAtStart,
                            sectionCurve[0].PointAtEnd);
                        Point3d center = hLine.PointAt(0.5);

                        //if the wall is intersecting we have to find intersection curve
                        Wall nW = new Wall();
                        nW.id = iw;
                        nW.section = w.section;
                        nW.material = w.material;
                        nW.name = w.name;


                        //construction lines
                        //longest horizontal
                        Line csl0 = hLine;
                        //middle line horizontal, perpendicular to wall
                        Line csl1 = new Line(
                            w.constructionLines[1].From,
                            w.constructionLines[1].To);
                        //vertical line
                        Line csl2 = new Line(
                            new Point3d(center.X, center.Y, bottomPlane.OriginZ),
                            new Point3d(center.X, center.Y, topPlane.OriginZ));
                        Transform move1 = new Transform();

                        Vector3d v1 = csl1.Direction * 0.5;
                        Vector3d v2 = csl1.Direction * -0.5;
                        csl1 = new Line(new Line(center, v1).To, new Line(center, v2).To);

                        nW.planeBottom = new Plane(csl2.From, new Vector3d(0, 0, 1));
                        nW.planeTop = new Plane(csl2.To, new Vector3d(0, 0, 1));
                        nW.constructionLines = new Line[] {
                        csl0,
                        csl1,
                        csl2};

                        //construction lines
                        Line c0 = nW.constructionLines[0]; //long horizontal
                        Line c1 = nW.constructionLines[1]; //width
                        Line c2 = nW.constructionLines[2]; //height

                        //make axis
                        var vecCenterDown = new Line(c2.PointAt(0.5), c2.From).Direction;
                        Curve railDownUp = new Line(c2.From, c2.To).ToNurbsCurve();

                        nW.bottomAxis = new Line(
                            new Point3d(c0.FromX, c0.FromY, c2.FromZ),
                            new Point3d(c0.ToX, c0.ToY, c2.FromZ)).ToNurbsCurve();

                        nW.topAxis = new Line(
                            new Point3d(c0.FromX, c0.FromY, c2.ToZ),
                            new Point3d(c0.ToX, c0.ToY, c2.ToZ)).ToNurbsCurve();
                        Curve rail = new Line(nW.bottomAxis.PointAtStart, nW.topAxis.PointAtStart).ToNurbsCurve();
                        Curve shape = nW.bottomAxis;
                        Brep srf = Brep.CreateFromSweep(rail, shape, true, 0.00001)[0];
                        nW.surface = srf;

                        newWalls.Add(iw, nW);
                        iw++;
                    }
                }
            }

            return newWalls;
        }
        //end of the method for splitting walls vertically


        //method creating a new lines based on intersection points
        public static List<Line> createNewLinesBasedOnIntersectionPoints( Line l, List<Point3d> ipts)
        {
            List<Line> cls = new List<Line>();  //cutting points
            double toler4 = 0.00001;

                //find points which are on the line
                List<Point3d> pointsOnLine = new List<Point3d>();
                foreach (Point3d ipt in ipts)
                {
                    double distFromLine = l.DistanceTo(ipt, true);
                    double distFromLineS = l.From.DistanceTo(ipt);
                    double distFromLineE = l.To.DistanceTo(ipt);

                    if (distFromLine < toler4 && distFromLineS > toler4 && distFromLineE > toler4)
                    {
                        pointsOnLine.Add(ipt);
                    }
                }

                if (pointsOnLine.Count > 0)
                {
                    //sort points on the line base on their distance from start of the line
                    Dictionary<double, Point3d> dictDistance = new Dictionary<double, Point3d>();
                    foreach (Point3d cpt in pointsOnLine)
                    {
                        double distFromStart = l.From.DistanceTo(cpt);
                        dictDistance.Add(distFromStart, cpt);
                    }

                    //sort points according to the distance from start point of the line
                    List<Point3d> sortedPoints = new List<Point3d>();
                    sortedPoints.Add(l.From);
                    foreach (var cp in dictDistance.OrderBy(i => i.Key))
                    {
                        sortedPoints.Add(cp.Value);
                    }
                    sortedPoints.Add(l.To);

                    for (int i = 0; i < sortedPoints.Count - 1; i++)
                    {
                        cls.Add(new Line(sortedPoints[i], sortedPoints[i + 1]));
                    }
                }
                else
                {
                    cls.Add(l);
                }

            return cls;
        }
        //end of method creating a new lines based on intersection points

        //method finding intersection of the lines in the specific range
        public static List<Point3d> findTheIntPoints(List<Line> lines, double tolerance1, double tolerance2)
        {
            double tolerance = tolerance1;
            List<Point3d> pts = new List<Point3d>();

            for (int i = 0; i < lines.Count; i++)
            {
                //list of lines
                List<Line> slines = new List<Line>();
                foreach (Line l in lines)
                {
                    slines.Add(l);
                }
                slines.RemoveAt(i);

                //list of lines without one
                for (int j = 0; j < slines.Count; j++)
                {
                    Line lineA = lines[i];
                    Line lineB = slines[j];
                    double pA;
                    double pB;


                    if (Rhino.Geometry.Intersect.Intersection.LineLine(lineA, lineB, out pA, out pB, 0.00001, false))
                    {
                        Point3d ptA = lineA.PointAt(pA);
                        Point3d ptB = lineB.PointAt(pB);

                        if (lineA.DistanceTo(ptA, true) < tolerance && lineB.DistanceTo(ptB, true) < tolerance)
                        {
                            double itolerance = tolerance2;
                            bool areTheyTouching = false;
                            if (
                              lineA.From.DistanceTo(lineB.From) < itolerance ||
                              lineA.From.DistanceTo(lineB.To) < itolerance ||
                              lineA.To.DistanceTo(lineB.From) < itolerance ||
                              lineA.To.DistanceTo(lineB.To) < itolerance)
                            {
                                areTheyTouching = true;
                            }

                            //info.Add("dist lA to lB 1 = " + lineA.From.DistanceTo(lineB.From));
                            //info.Add("dist lA to lB 2= " + lineA.From.DistanceTo(lineB.To));
                            //info.Add("dist lA to lB 3= " + lineA.To.DistanceTo(lineB.From));
                            //info.Add("dist lA to lB 4= " + lineA.To.DistanceTo(lineB.To));
                            //info.Add("areTheyTouching= " + areTheyTouching);

                            if (!areTheyTouching)
                            {
                                //info.Add("add point = " + !areTheyTouching);
                                pts.Add(ptA);
                            }
                        }
                    }
                }
            }
            pts = Point3d.CullDuplicates(pts, 0.000001).ToList();
            return pts;
        }
        // end of the method which finds intersection points

        // method which extrud or trim lines according to intersection points
        public static Line adjustLineToIntPoints(Line l, List<Point3d> ipts, double tolerance3)
        {
            //adjust line to int points (ipts)
            Point3d sPt = l.From;
            Point3d ePt = l.To;

            foreach (Point3d iPt in ipts)
            {
                double dSI = iPt.DistanceTo(sPt);
                double dEI = iPt.DistanceTo(ePt);

                //check if start point is close to int point
                if (dSI < tolerance3)
                {
                    sPt = iPt;
                }

                //check if end point is close to int point
                if (dEI < tolerance3)
                {
                    ePt = iPt;
                }
            }
            var oline = new Line(sPt, ePt);

            return oline;
        }
        //end of  method which extrud or trim lines according to intersection points

        //medhod adjust the walls
        public static Dictionary<int, Wall> adjustWalls(Dictionary<int, Wall> walls, double[] wallTolerances)
        {
            Dictionary<int, Wall> newWalls = new Dictionary<int, Wall>();

            List<Line> lines = new List<Line>();
            foreach (var w in walls)
            {
                lines.Add(new Line(w.Value.bottomAxis.PointAtStart, w.Value.bottomAxis.PointAtEnd));
            }

            List<Point3d> ipts =findTheIntPoints(lines, wallTolerances[0], wallTolerances[1]);
            int iw = 0;
            foreach (var w in walls)
            {
                Line oldAxis = new Line(w.Value.bottomAxis.PointAtStart, w.Value.bottomAxis.PointAtEnd);
                Line adjustedToipts = adjustLineToIntPoints(oldAxis, ipts, wallTolerances[2]);
                var newAxis = createNewLinesBasedOnIntersectionPoints(adjustedToipts, ipts);

                if (newAxis.Count == 1)
                {
                    Wall newWall = new Wall();
                    newWall.name = w.Value.name;
                    newWall.id = iw++;
                    newWall.material = w.Value.material;
                    newWall.bottomAxis = newAxis[0].ToNurbsCurve();
                    newWall.planeBottom = w.Value.planeBottom;
                    newWall.planeTop = w.Value.planeTop;
                    newWall.plane = w.Value.plane;
                    newWall.section = w.Value.section;
                    newWall.topAxis = new Line(
                        new Point3d(newAxis[0].FromX, newAxis[0].FromY, w.Value.planeTop.OriginZ),
                        new Point3d(newAxis[0].ToX, newAxis[0].ToY, w.Value.planeTop.OriginZ)).ToNurbsCurve();
                    newWall.constructionLines = w.Value.constructionLines;
                    newWalls.Add(iw, newWall);
                }
                else
                {
                    foreach (var nw in newAxis)
                    {
                        Wall newWall = new Wall();
                        newWall.name = w.Value.name;
                        newWall.id = iw++;
                        newWall.material = w.Value.material;
                        newWall.bottomAxis = nw.ToNurbsCurve();
                        newWall.planeBottom = w.Value.planeBottom;
                        newWall.planeTop = w.Value.planeTop;
                        newWall.plane = w.Value.plane;
                        newWall.section = w.Value.section;
                        newWall.topAxis = new Line(
                            new Point3d(nw.FromX, nw.FromY, w.Value.planeTop.OriginZ),
                            new Point3d(nw.ToX, nw.ToY, w.Value.planeTop.OriginZ)).ToNurbsCurve();
                        newWall.constructionLines = w.Value.constructionLines;
                        newWalls.Add(iw, newWall);
                    }
                }
            }

            return newWalls;
        }
        //end of method adjust the walls

        public static Floor adjustFloorToBottomWalls(Floor floor, Dictionary<int, Wall> walls, double tolerance)
        {
            Floor newFloor = new Floor();
            //z coordinate of the plane
            double zc = Math.Round(floor.plane.OriginZ,5);

            List<Line> bLs = new List<Line>();
            List<Line> tLs = new List<Line>();
            List<Point3d> bPs = new List<Point3d>();
            List<Point3d> tPs = new List<Point3d>();
            //create a list of wall lines, top and bottom ones, which are touching floor
            foreach (var w in walls.Values)
            {
                double bz = Math.Round(w.bottomAxis.PointAtEnd.Z,5) ;

                if (bz == zc)
                {
                    //create new line and add it bottom lines
                    bLs.Add(new Line(w.bottomAxis.PointAtStart, w.bottomAxis.PointAtEnd));
                    bPs.Add(w.bottomAxis.PointAtStart);
                    bPs.Add(w.bottomAxis.PointAtEnd);
                }
            }
            //clean point lists from duplicates

            if (bPs.Count > 0)
            {
                var bpts = Point3d.CullDuplicates(bPs, 0.0001).ToList();
                //var tpts = Point3d.CullDuplicates(tPs, tolerance).ToList();

                //create polyline from external boundary of floor
                Polyline pl = new Polyline();
                floor.boundaryExternal.TryGetPolyline(out pl);

                //correct external polyline
                var moved = moveBoundaryToInLines(pl, bLs, tolerance);
                var corrected = correctLinesAndCreatePolyline(moved, tolerance);
                var adjusted = adjustPolylineToPoints(corrected, bpts, tolerance);

                newFloor.name = floor.name;
                newFloor.material = floor.material;
                newFloor.boundaryExternal = adjusted.ToNurbsCurve();

                //correct internal polyline
                List<Curve> pls = new List<Curve>();
                foreach (var ib in floor.boundaryInternal)
                {
                    Polyline pli = new Polyline();
                    ib.TryGetPolyline(out pli);
                    var movedi = moveBoundaryToInLines(pli, bLs, tolerance);
                    var correctedi = correctLinesAndCreatePolyline(movedi, tolerance);
                    var adjustedi = adjustPolylineToPoints(correctedi, bpts, tolerance);

                    pls.Add(adjustedi.ToNurbsCurve());
                }

                newFloor.boundaryInternal = pls.ToArray();
                
                List<Curve> allCurves = new List<Curve>();
                allCurves.Add(newFloor.boundaryExternal);
                allCurves.AddRange(newFloor.boundaryInternal);
                newFloor.surface = Brep.CreatePlanarBreps(allCurves, 0.0001);
            }
            else
            {
                newFloor = floor;
            }

            return newFloor;
        }


        public static Floor adjustFloorToTopWalls(Floor floor, Dictionary<int, Wall> walls, double tolerance)
        {
            Floor newFloor = new Floor();
            //z coordinate of the plane
            double zc = Math.Round(floor.plane.OriginZ, 5);

            List<Line> bLs = new List<Line>();
            List<Line> tLs = new List<Line>();
            List<Point3d> bPs = new List<Point3d>();
            List<Point3d> tPs = new List<Point3d>();
            //create a list of wall lines, top and bottom ones, which are touching floor
            foreach (var w in walls.Values)
            {
                double bz = Math.Round(w.topAxis.PointAtEnd.Z, 5);

                if (bz == zc)
                {
                    //create new line and add it bottom lines
                    bLs.Add(new Line(w.topAxis.PointAtStart, w.topAxis.PointAtEnd));
                    bPs.Add(w.topAxis.PointAtStart);
                    bPs.Add(w.topAxis.PointAtEnd);
                }
            }
            //clean point lists from duplicates

            if (bPs.Count > 0)
            {
                var bpts = Point3d.CullDuplicates(bPs, 0.0001).ToList();
                //var tpts = Point3d.CullDuplicates(tPs, tolerance).ToList();

                //create polyline from external boundary of floor
                Polyline pl = new Polyline();
                floor.boundaryExternal.TryGetPolyline(out pl);

                //correct external polyline
                var moved = moveBoundaryToInLines(pl, bLs, tolerance);
                var corrected = correctLinesAndCreatePolyline(moved, tolerance);
                var adjusted = adjustPolylineToPoints(corrected, bpts, tolerance);

                newFloor.name = floor.name;
                newFloor.material = floor.material;
                newFloor.boundaryExternal = adjusted.ToNurbsCurve();

                //correct internal polyline
                List<Curve> pls = new List<Curve>();
                foreach (var ib in floor.boundaryInternal)
                {
                    Polyline pli = new Polyline();
                    ib.TryGetPolyline(out pli);
                    var movedi = moveBoundaryToInLines(pli, bLs, tolerance);
                    var correctedi = correctLinesAndCreatePolyline(movedi, tolerance);
                    var adjustedi = adjustPolylineToPoints(correctedi, bpts, tolerance);

                    pls.Add(adjustedi.ToNurbsCurve());
                }

                newFloor.boundaryInternal = pls.ToArray();

                List<Curve> allCurves = new List<Curve>();
                allCurves.Add(newFloor.boundaryExternal);
                allCurves.AddRange(newFloor.boundaryInternal);
                newFloor.surface = Brep.CreatePlanarBreps(allCurves, 0.0001);
            }
            else
            {
                newFloor = floor;
            }

            return newFloor;
        }


        //correct lines
        public static Polyline correctLinesAndCreatePolyline(List<Line> lines, double tolerance)
        {
            int numOfLs = lines.Count;
            //the first and last line
            Line firstLine = lines[0];
            Line lastLine = lines[numOfLs - 1];

            double pa;
            double pb;

            Rhino.Geometry.Intersect.Intersection.LineLine(lastLine, firstLine, out pa, out pb, tolerance, false);

            Point3d start = lastLine.PointAt(pa);

            List<Point3d> ipts = new List<Point3d>();
            ipts.Add(start);

            for (int i = 0; i < numOfLs - 1; i++)
            {
                Line tempL = lines[i];
                Line nextL = lines[i + 1];

                double pt;
                double pn;

                Rhino.Geometry.Intersect.Intersection.LineLine(tempL, nextL, out pt, out pn, tolerance, false);

                ipts.Add(tempL.PointAt(pt));
            }

            ipts.Add(start);

            Polyline pl = new Polyline(ipts);
            return pl;
        }

        //move lines to boundaries
        public static List<Line> moveBoundaryToInLines(Polyline plfl, List<Line> lines, double tolerance)
        {
            List<Line> movedLines = new List<Line>();
            for (int i = 0; i < plfl.Count - 1; i++)
            {
                //
                var sP = plfl[i];
                var eP = plfl[i + 1];
                Line segment = new Line(sP, eP);
                //
                Vector3d segVec = segment.Direction;

                Line mL = segment;
                foreach (Line sW in lines)
                {
                    Vector3d wallVec = sW.Direction;

                    if (segVec.IsParallelTo(wallVec, 0.05) == 1 || segVec.IsParallelTo(wallVec, 0.05) == -1) //approximately 3.6 deg of angle difference is allowed
                    {
                        double mindist = segment.MinimumDistanceTo(sW);
                        if (mindist < tolerance)
                        {
                            mL = sW;
                        }
                    }
                }
                movedLines.Add(mL);
            }
            return movedLines;
        }


        //adjust polyline vertices to set of points
        public static Polyline adjustPolylineToPoints(Polyline plfl, List<Point3d> allPts, double tolerance)
        {
            //
            Polyline npl = new Polyline();

            for (int i = 0; i < plfl.Count; i++)
            {
                Point3d tpt = new Point3d(plfl[i]);
                foreach (Point3d ap in allPts)
                {
                    double dist = plfl[i].DistanceTo(ap);
                    if (dist < tolerance)
                    {
                        tpt = ap;
                    }

                }

                npl.Add(tpt);
            }

            return npl;
        }


        //clean the holes in the floor
        public static Floor correctHolesInTheFloor(Floor floor)
        {
            //

            var iholes = floor.boundaryInternal;
            var mergedholes = Curve.CreateBooleanUnion(iholes, 0.0001);

            Curve boundary = floor.boundaryExternal;
            double t = 0.01;
            List<Curve> curvesNotSubtract = new List<Curve>();
            List<Curve> curvesToSubtract = new List<Curve>();

            foreach (Curve mh in mergedholes)
            {
                Polyline hole = new Polyline();
                mh.TryGetPolyline(out hole);
                List<Point3d> pointsTouchingBoundary = new List<Point3d>();

                foreach (Point3d ph in hole)
                {
                    if (boundary.Contains(ph, Plane.WorldXY, t) == PointContainment.Coincident)
                    { 
                        pointsTouchingBoundary.Add(ph);
                    }    
                }

                if (pointsTouchingBoundary.Count > 1)
                {
                    curvesToSubtract.Add(mh);
                }
                else
                {
                    curvesNotSubtract.Add(mh);
                }
            
            }


            var substractHoles = Curve.CreateBooleanDifference(boundary, curvesToSubtract, 0.0001);

            if (substractHoles.Length>0)
            {
                floor.boundaryExternal = Curve.CreateBooleanDifference(boundary, curvesToSubtract, 0.0001)[0];
            }
            else
            {
                floor.boundaryExternal = boundary;
            }
            
            floor.boundaryInternal = curvesNotSubtract.ToArray();

            floor.rebuildSurfaceBasedOnBoundaries(); 

            //floor.boundaryInternal = null;

            return floor;
        }

        //end of allmethods
    }
}
