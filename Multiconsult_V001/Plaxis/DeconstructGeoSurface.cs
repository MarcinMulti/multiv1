using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Multiconsult_V001.Plaxis
{
    public class DeconstructGeoSurface : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructGeoSurface class.
        /// </summary>
        public DeconstructGeoSurface()
          : base("DeconstructGeoSurface", "Nickname",
              "Description",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GeoSurfaces","","",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("name", "", "", GH_ParamAccess.item); //0
            pManager.AddIntegerParameter("id", "", "", GH_ParamAccess.item); //1
            pManager.AddPointParameter("points", "", "", GH_ParamAccess.list); //2
            pManager.AddGenericParameter("nodes","","",GH_ParamAccess.list); //3
            pManager.AddBrepParameter("surface", "", "", GH_ParamAccess.item); //4
            pManager.AddMeshParameter("mesh", "", "", GH_ParamAccess.item); //5
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Multiconsult_V001.Classes.Geo_Surface gs = new Classes.Geo_Surface();
            DA.GetData(0,ref gs);

            DA.SetData(0, gs.name);
            DA.SetData(1, gs.id);
            DA.SetDataList(2, gs.nodes.Select(x => x.point).ToList());
            DA.SetDataList(3, gs.nodes);
            DA.SetData(4, gs.surface);
            DA.SetData(5, gs.mesh);
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
            get { return new Guid("b89bf55a-412b-4b4d-a680-076775508632"); }
        }
    }
}