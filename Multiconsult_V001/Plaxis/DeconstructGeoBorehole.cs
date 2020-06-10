using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Plaxis
{
    public class DeconstructGeoBorehole : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructGeoBorehole class.
        /// </summary>
        public DeconstructGeoBorehole()
          : base("DeconstructGeoBorehole", "Nickname",
              "Description",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Borehole","GB","", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("id", "I", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Location", "L", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "N", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Geo_Borehole gb = new Geo_Borehole();
            DA.GetData(0, ref gb);

            DA.SetData(0,gb.id);
            DA.SetData(1, gb.position);
            DA.SetData(2, gb.name);
            DA.SetDataList(3, gb.points);
            DA.SetDataList(4, gb.nodes);
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
            get { return new Guid("1341900d-ad87-42e2-987c-768731a7dd49"); }
        }
    }
}