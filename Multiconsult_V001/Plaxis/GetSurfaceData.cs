using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Geometry;
using Multiconsult_V001.Classes;
using Rhino.Geometry.Intersect;

namespace Multiconsult_V001.Plaxis
{
    public class GetSurfaceData : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetSurfaceData class.
        /// </summary>
        public GetSurfaceData()
          : base("GetSurfaceData", "Nickname",
              "Description",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("iPts", "iP", "Input points", GH_ParamAccess.list);
            pManager.AddCurveParameter("iCrv", "iC", "The boundary curve, the boundary on which the geo surface should be created", GH_ParamAccess.item);
            pManager.AddNumberParameter("precision","PR","How big the intial grid should be",GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("tolerance", "TO", "Tolerance of culling input points", GH_ParamAccess.item, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("GEoSurface","GS","Geo Surface class", GH_ParamAccess.item);
            pManager.AddBrepParameter("Terrain","TB","Brep representing terrain",GH_ParamAccess.item);
            pManager.AddPointParameter("oPts", "oP", "Brep representing terrain", GH_ParamAccess.list);

            pManager.AddTextParameter("oInf", "oI", "Brep representing terrain", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> gpts = new List<Point3d>();
            Curve crv = new Line().ToNurbsCurve();
            double prec = 10;
            double tole = 0.1;

            DA.GetDataList(0,gpts);
            DA.GetData(1, ref crv);
            DA.GetData(2, ref prec);
            DA.GetData(3, ref tole);

            Polyline pl = new Polyline();
            crv.TryGetPolyline(out pl);
            gpts = Point3d.CullDuplicates(gpts,tole).ToList();

            //create basic point grid
            var gridPts = createGridOfFlatPoints(pl, prec);
            //creat spatial distributed grid of points
            var spatPts = createGridOfSpatialPoints(gridPts, gpts);

            //divide region into grid of defined span
            int n1 = Convert.ToInt32(new Line(pl[0], pl[1]).Length / prec);
            int n2 = Convert.ToInt32(new Line(pl[1], pl[2]).Length / prec);

            NurbsSurface nS = NurbsSurface.CreateFromPoints(spatPts, n1+1, n2+1, 2, 2);
            Brep bS = nS.ToBrep();

            DA.SetData(1,bS);
            DA.SetDataList(2, spatPts);
            //DA.SetDataList(3, info);
        }
        public List<Point3d> createGridOfSpatialPoints(List<Point3d> flatGridPoints, List<Point3d> pointsFromGeoLayer)
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
        public List<Point3d> createGridOfFlatPoints(Polyline pl, double prec)
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


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cf7c63c4-9938-41e3-97eb-1090628bd60c"); }
        }
    }
}