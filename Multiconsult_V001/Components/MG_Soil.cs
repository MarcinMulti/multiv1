using System;
using System.Collections.Generic;
using Multiconsult_V001.Classes;
using Multiconsult_V001.Methods;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Multiconsult_V001.Components
{
    public class MG_Soil : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MG_Soil()
          : base("ReadSoil", "RS",
              "Creare Soil from string list",
              "Multiconsult", "Geotechnic")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Strings", "ST", "The string  list with coordinates information", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Soil", "S", "Model of the soil, closed in Geo_Soil class", GH_ParamAccess.item);
            pManager.AddTextParameter("Infos", "I", "Information about terrain creation", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points on the terrain", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Surface", "S", "Surface of the terrain", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<string> strings = new List<string>();
            DA.GetDataList(0, strings);

            //variables
            List<string> infos = new List<string>();
            infos.Add("The process started");

            //methods
            //create points on terrain base on string list from csv file, comma is the separtor, coord X is 1, coord Y is 2 and coord Z is 3 of the point
            var pts = CreateListOfPoints(strings);

           
            var soil = new Geo_Layer();

            DA.SetData(0, soil);
            DA.SetDataList(1, infos);
            DA.SetDataList(2, pts);
        }


        public List<Point3d> CreateListOfPoints(List<string> strs)
        {
            //variables
            List<Point3d> pts = new List<Point3d>();

            //sort over the strings
            foreach (var str in strs)
            {
                //split the string if there is a comma, the list is from csv file, so if the separtor changes, this have to be switched
                char[] separators = { ',' };
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
            get { return new Guid("1d8c72ad-61e5-4cf4-a96b-3c9e10134a7d"); }
        }
    }
}