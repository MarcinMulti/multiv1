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

            Line hLine = new Line(cpts[0],cpts[1]);
            var moveToCenter = Transform.Translation((Point3d.Subtract(hLine.PointAt(0.5), longest.PointAt(0.5))));
            hLine.Transform(moveToCenter);

            lines[0] = longest;
            lines[1] = midleLine;
            lines[2] = hLine;

            //outputs
            return lines;
        }
    }
}
