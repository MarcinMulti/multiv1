using System;
using System.Collections.Generic;
using Multiconsult_V001.Classes;
using Multiconsult_V001.Methods;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace Multiconsult_V001.Components
{
    public class MG_Terrain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent4 class.
        /// </summary>
        public MG_Terrain()
          : base("ReadTerrain", "RT",
              "Creare Terrain from string list",
              "Multiconsult", "Geotechnic")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Strings","ST","The string  list with coordinates information",GH_ParamAccess.list);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Terrain", "T", "Model of the terrain, closed in Geo_Terrain class", GH_ParamAccess.item) ;
            pManager.AddTextParameter("Infos", "I", "Information about terrain creation", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points on the terrain", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Surface", "S", "Surface of the terrain", GH_ParamAccess.list);
            pManager.AddCurveParameter("Hull", "H", "Curve of the terrain", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angle", "ang", "The string  list with coordinates information", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<string> strings = new List<string>();
            DA.GetDataList(0,strings);

            //variables
            List<string> infos = new List<string>();
            infos.Add("The process started");

            //methods
            //create points on terrain base on string list from csv file, comma is the separtor, coord X is 1, coord Y is 2 and coord Z is 3 of the point
            var pts = CreateListOfPoints(strings);

            var terrain = new Geo_Terrain(pts);

            //project point on XY plane
            var fpts = Methods.Plaxis.ProjectPointsOnXY(pts);

            var ghfpts = new List<GH_Point>();
            foreach (var fp in fpts)
            {
                ghfpts.Add(new GH_Point(fp));
            }

            //mesh
            var hull = Grasshopper.Kernel.Geometry.ConvexHull.Solver.ComputeHull(ghfpts);
            Curve crv = hull.ToNurbsCurve();
            Curve[] crvs = new Curve[] { crv };
            Brep b1 = Brep.CreateTrimmedPlane(Plane.WorldXY, crv);
            Brep b2 = Brep.CreatePlanarBreps(crvs,0.00001)[0];
            List<Surface> srfs = new List<Surface>();
            
            double ang = 2*Math.PI;
            int precision = 20;
            double dang = ang / Convert.ToDouble(precision);
            List<double> angles = new List<double>();
            List<Curve> pls = new List<Curve>();

            for (int i = 0; i < precision; i++)
            {
                double angle = dang * i;
                angles.Add(angle);
            }

            Dictionary<double, double> dicAngleArea = new Dictionary<double, double>();

            foreach (var a in angles)
            {

                Polyline pl1 = new Polyline();
                crv.TryGetPolyline(out pl1);
                var t = Transform.Rotation(a, new Point3d(0,0,0));
                pl1.Transform(t);
                var bbpl1 = new BoundingBox(pl1);

                
                var a1 = AreaMassProperties.Compute(pl1.ToNurbsCurve()).Area;
                var a2 = bbpl1.Area;

                double difA = Math.Abs(a1 - a2);
                dicAngleArea.Add(a, difA);

                pls.Add(pl1.ToNurbsCurve());
                var lbbpl1 = bbpl1.GetCorners();
                pls.Add(new Polyline(lbbpl1).ToNurbsCurve());
            }


            //take the best rotation angle, which means the rotation to fit the bounding box the best
            var keyAngle = dicAngleArea.OrderBy(kvp => kvp.Value).First().Key;


            //outputs
            DA.SetData(0, terrain);
            DA.SetDataList(1, infos);
            DA.SetDataList(2, pts);
            DA.SetDataList(3, srfs);
            DA.SetDataList(4, pls);
            DA.SetData(5, keyAngle);
        }

        public List<Point3d> CreateListOfPoints(List<string> strs)
        {
            //variables
            List<Point3d> pts = new List<Point3d>();

            //sort over the strings
            foreach (var str in strs)
            {
                //split the string if there is a comma, the list is from csv file, so if the separtor changes, this have to be switched
                char[] separators = {','};
                Int32 count = 7; //this is maximum amount of data, normally there is just 5 so to be sure that everything will be splitted I take 7, like 7 sins :)
                var typeOfSplit = StringSplitOptions.RemoveEmptyEntries;

                //split by char
                String[] strlist = str.Split(separators, count, StringSplitOptions.RemoveEmptyEntries); 

                //create coords and Point3d
                double x = Convert.ToDouble(strlist[1]);
                double y = Convert.ToDouble(strlist[2]);
                double z = Convert.ToDouble(strlist[3]);
                pts.Add(new Point3d(x, y, z));
            }

            //output
            return pts; 
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
            get { return new Guid("e122b3aa-a5ab-4908-a9c2-fa86cc0c082f"); }
        }
    }
}