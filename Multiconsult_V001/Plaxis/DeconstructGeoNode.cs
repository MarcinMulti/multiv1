using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Plaxis
{
    public class DeconstructGeoNode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructGeoNode class.
        /// </summary>
        public DeconstructGeoNode()
          : base("DeconstructGeoNode", "Nickname",
              "Description",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GeoNode", "GN", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Id","I","", GH_ParamAccess.item);
            pManager.AddTextParameter("Name1", "N1", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Name2", "N2", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Geo_Node gn = new Geo_Node();
            DA.GetData(0, ref gn);

            DA.SetData(0, gn.id);
            DA.SetData(1, gn.name1);
            DA.SetData(2, gn.name2);
            DA.SetData(3, gn.point);
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
            get { return new Guid("f5e9d219-b623-4bda-9b63-3de8724d2534"); }
        }
    }
}