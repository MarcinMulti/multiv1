using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Multiconsult_V001.Classes;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Linq;

namespace Multiconsult_V001.Plaxis
{
    public class MP_BuildLayers : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BuildSoil class.
        /// </summary>
        public MP_BuildLayers()
          : base("BuildLayers", "Nickname",
              "Description",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GeoSoil","GS","Object GeoSoil class", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("GeoSoil", "GS", "Object GeoSoil class", GH_ParamAccess.item);
            pManager.AddGenericParameter("GeoLayers", "GLs", "Object GeoLayer class", GH_ParamAccess.list);
            pManager.AddBrepParameter("breps", "B", "breps", GH_ParamAccess.list);
            pManager.AddTextParameter("info", "I", "infos", GH_ParamAccess.list);
            pManager.AddBrepParameter("breps", "B", "breps", GH_ParamAccess.list);
            pManager.AddBrepParameter("breps", "B", "breps", GH_ParamAccess.list);
            pManager.AddBrepParameter("breps", "B", "breps", GH_ParamAccess.list);
            pManager.AddCurveParameter("crvs", "C", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Geo_Soil gs = new Geo_Soil();
            DA.GetData(0, ref gs);

            List<Geo_Layer> gls = new List<Geo_Layer>();
            List<Brep> bs = new List<Brep>();
            List<string> info = new List<string>();
            List<Brep> bs1 = new List<Brep>();
            List<Brep> bs2 = new List<Brep>();
            List<Brep> bs3 = new List<Brep>();
            List<Curve> cs = new List<Curve>();

            info.Add("Create volume layers from Geo_Soil");

            foreach (var s in gs.geosurfaces)
            {
                bs.Add(s.surface);
            }

            foreach (var s in gs.geosurfaces)
            {
                foreach (var b1 in bs)
                {
                    Curve[] icrvs;
                    Point3d[] ipts;
                    var isInt = Intersection.BrepBrep(s.surface, b1, 0.1, out icrvs, out ipts);
                    info.Add("is intersection =" + isInt);
                    info.Add("number of curves =" + icrvs.Length);
                    if ( icrvs.Length>0 )
                    { 
                        var bss = s.surface.Split(b1, 0.1);
                        bs1.AddRange(bss);
                    }
                }
            }

            DA.SetData(0, gs);
            DA.SetDataList(1, gls);
            DA.SetDataList(2, bs);
            DA.SetDataList(3, info);
            DA.SetDataList(4, bs1);
            DA.SetDataList(5, bs2);
            DA.SetDataList(6, bs3);
            DA.SetDataList(7, cs);
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
            get { return new Guid("12d667db-e89b-4ad2-8838-00066fafe82c"); }
        }
    }
}