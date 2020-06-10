using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Multiconsult_V001.Classes;
using Rhino.Geometry.Intersect;

namespace Multiconsult_V001.Methods
{
    class Plaxis
    {

        //method to zero the z coordinates in list od points
        public static List<Point3d> ProjectPointsOnXY(List<Point3d> pts)
        {            
            List<Point3d> fPts = new List<Point3d>();

            foreach (var p in pts)
            {
                fPts.Add(new Point3d(p.X,p.Y,0));
            }

            return fPts;
        }

        public static List<Point3d> createGridOfSpatialPoints(List<Point3d> flatGridPoints, List<Point3d> pointsFromGeoLayer)
        {
            var gridPts = flatGridPoints;
            var gpts = pointsFromGeoLayer;
            //create spatial grid
            List<Point3d> allPts = new List<Point3d>(); //spatial grid points
            int iP = 0;
            foreach (var gP in gridPts)
            {
                Dictionary<Point3d, double> dicPointDist = new Dictionary<Point3d, double>();
                foreach (var dP in gpts)
                {
                    //create dictionary, which connects geopoint with distance to artifical node
                    Point3d fdP = new Point3d(dP.X, dP.Y, 0);
                    double dist = gP.DistanceTo(fdP);
                    dicPointDist.Add(dP, dist);
                }
                //find two the closest points
                Dictionary<Point3d, double> sdicPointDist = dicPointDist.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                Point3d p1 = sdicPointDist.Keys.ToList()[0];
                Point3d p2 = sdicPointDist.Keys.ToList()[1];
                double dist1 = sdicPointDist[p1];
                double dist2 = sdicPointDist[p2];
                double perc1 = 1 - dist1 / (dist1 + dist2);
                double perc2 = 1 - dist2 / (dist1 + dist2);

                double avgZ = Math.Round((perc2 * p2.Z + perc1 * p1.Z), 3);
                Point3d aP = new Point3d(gP.X, gP.Y, avgZ);
                allPts.Add(aP);
                /*info.Add("Point id =" + iP);
                info.Add("dist1 = " + dist1 + "perc1 = " + perc1 + " p1.Z = " + p1.Z);
                info.Add("dist2 = " + dist2 + "perc2 = " + perc2 + " p2.Z = " + p2.Z);
                info.Add("avgZ = " + avgZ);
                info.Add("aP  X=" + aP.X + " Y=" + aP.Y + " Z=" + aP.Z);
                */
                iP++;
            }
            return allPts;
        }
        public static List<Point3d> createGridOfFlatPoints(Polyline pl, double prec)
        {
            double span = prec;
            Line l1 = new Line(pl[0], pl[1]);
            Line l2 = new Line(pl[1], pl[2]);
            Line l3 = new Line(pl[3], pl[2]);
            Line l4 = new Line(pl[0], pl[3]);
            //divide region into grid of defined span
            double dl1 = l1.Length / span;
            double dl2 = l2.Length / span;

            int n1 = Convert.ToInt32(dl1);
            int n2 = Convert.ToInt32(dl2);

            double d1 = 1 / Convert.ToDouble(n1);
            double d2 = 1 / Convert.ToDouble(n2);

            List<Point3d> gridPts = new List<Point3d>(); //flat grid points
            //create flat grid of points
            for (int i1 = 0; i1 < n1 + 1; i1++)
            {
                Line lineToDivide = new Line(l1.PointAt(i1 * d1), l3.PointAt(i1 * d1));
                for (int i2 = 0; i2 < n2 + 1; i2++)
                {
                    Point3d aPt = lineToDivide.PointAt(i2 * d2);
                    gridPts.Add(new Point3d(aPt.X, aPt.Y, 0));
                }
            }

            return gridPts;
        }

        public static Mesh createDelMeshFromPoints(List<Point3d> pts)
        {
            Node2List n2l = new Node2List();
            foreach (var pt in pts)
            {
                n2l.Append(new Node2(pt.X, pt.Y));
            }
            var delMesh = new Mesh();
            var faces = new List<Grasshopper.Kernel.Geometry.Delaunay.Face>();

            faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(n2l, 1);
            delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(n2l, 1, ref faces);

            for (int i = 0; i < delMesh.Vertices.Count; i++)
            {
                delMesh.Vertices[i] = new Point3f(
                    float.Parse(pts[i].X.ToString()),
                    float.Parse(pts[i].Y.ToString()),
                    float.Parse(pts[i].Z.ToString())
                    );
            }

            return delMesh;
        }

        public static Mesh createRectangleMesh(List<Point3d> pts , int n1, int n2)
        {
            Mesh m = new Mesh();

            foreach (var pt in pts)
            {
                m.Vertices.Add(pt);
            }
            int v1 = 0;
            int v2 = 0;
            int v3 = 0;
            int v4 = 0;

            int v12 = 0;
            int v22 = 1;
            int v32 = n1+2;
            int v42 = n1+1;

            for (int i2 = 0; i2 < n2; i2++)
            {
                for (int i1 = 0; i1 < n1; i1++)
                {
                    v1 = i1 + v12;
                    v2 = i1 + v22;
                    v3 = i1 + v32;
                    v4 = i1 + v42;
                    m.Faces.AddFace(v1, v2, v3, v4);
                }
                v12 = (n1+1) * (i2+1);
                v22 = (n1 + 1) * (i2+1)+1;
                v32 = (n1 + 1) * (i2+2)+1;
                v42 = (n1 + 1) * (i2+2);
            }

            //m.FaceNormals.ComputeFaceNormals();
            m.Compact();

            return m;
        }
    }

}
