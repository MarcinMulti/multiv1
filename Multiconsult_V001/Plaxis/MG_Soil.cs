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
    public class MG_Soil : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MG_Soil()
          : base("ReadSoil", "RS",
              "Creare Soil from string list",
              "Multiconsult", "Plaxis")
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
            pManager.AddGenericParameter("Soil", "S", "Model of the soil, closed in Geo_Soil class", GH_ParamAccess.item); //0
            pManager.AddTextParameter("Infos", "I", "Information about terrain creation", GH_ParamAccess.list);//1
            pManager.AddPointParameter("Points", "P", "Points on the terrain", GH_ParamAccess.list);//2
            pManager.AddSurfaceParameter("Surface", "S", "Surface of the terrain", GH_ParamAccess.list);//3
            pManager.AddGenericParameter("GeoSurfaces", "S", "Model of the soil, closed in Geo_Soil class", GH_ParamAccess.list);//4
            pManager.AddGenericParameter("GeoBoreholes", "S", "Model of the soil, closed in Geo_Soil class", GH_ParamAccess.list);//5
            pManager.AddGenericParameter("GeoLayers", "S", "Model of the soil, closed in Geo_Soil class", GH_ParamAccess.list);//6
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
            List<Geo_Node> geonodes = CreateListOfGeoNodes(strings);
            var pts = new List<Point3d>();
            var lys = new List<string>();

            foreach (var node in geonodes)
            { 
                    pts.Add(node.point);
                    lys.Add(node.name2);
            }

            //Construct basic geosurfaces
            List<Geo_Surface> gsrfs = new List<Geo_Surface>();
            int gs = 0;
            foreach (var layerName in lys.Distinct())
            {
                Geo_Surface gsrf = new Geo_Surface();
                gsrf.id = gs++;
                gsrf.name = layerName;
                gsrf.nodes = new List<Geo_Node>();
                foreach (var gn in geonodes)
                {
                    if (gn.name2 == layerName)
                        gsrf.nodes.Add(gn);
                }
                gsrfs.Add(gsrf);
            }

            //Construct basic geoboreholes
            List<Geo_Borehole> gbhs = new List<Geo_Borehole>();
            List<Point3d> fpt = new List<Point3d>();
            foreach (var pt in pts)
            {
                fpt.Add(new Point3d(pt.X, pt.Y, 0));
            }

            var uniquePoints = Point3d.CullDuplicates(fpt, 0.1);
            int igbh = 0;
            foreach (var upt in uniquePoints)
            {
                Geo_Borehole gbh = new Geo_Borehole();
                gbh.name = "vertical borehole";
                gbh.id = igbh++;
                gbh.position = upt;
                gbh.nodes = new List<Geo_Node>();

                //add geo nodes
                foreach (var gn in geonodes)
                {
                    if (Math.Abs(gn.point.X-upt.X)<1 && Math.Abs(gn.point.Y-upt.Y)<1)
                        gbh.nodes.Add(gn);
                }
                gbhs.Add(gbh);
            }

            //cleaning boreholes from strange data
            List<Geo_Borehole> cgbhs = new List<Geo_Borehole>(); // cleaned boreholes
            foreach (var bh in gbhs)
            {
                if (bh.nodes.Count > 2)
                    cgbhs.Add(bh); //take only boreholes with at least 3 geonodes
            }

            //creating geo layers
            List<Geo_Layer> glys = new List<Geo_Layer>();

            infos.AddRange(lys.Distinct());    

            


            var soil = new List<Geo_Layer>();

            DA.SetDataList(0, soil);
            DA.SetDataList(1, infos);
            DA.SetDataList(2, pts); 
            //DA.SetDataList(3, pts); //surfaces
            DA.SetDataList(4, gsrfs); //geosurface
            DA.SetDataList(5, cgbhs); //geoborhole
            DA.SetDataList(6, glys); //geoLayer
        }

        public List<Geo_Node> CreateListOfGeoNodes( List<string> strs )
        {
            //variables
            List<Geo_Node> nodes = new List<Geo_Node>();

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
                if (strlist.Length ==5)
                { 
                Geo_Node gn = new Geo_Node();
                gn.name2 = strlist[4];
                gn.point = new Point3d(x, y, z);
                nodes.Add(gn);
                }
            }

            //output
            return nodes;
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