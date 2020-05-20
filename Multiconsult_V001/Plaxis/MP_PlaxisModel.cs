using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Multiconsult_V001.Plaxis
{
    public class MP_PlaxisModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MP_PlaxisModel class.
        /// </summary>
        public MP_PlaxisModel()
          : base("MP_PlaxisModel", "Nickname",
              "Description",
              "Category", "Subcategory")
        {        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {pManager.AddGenericParameter("PlaxisModel", "MA", "Multiconsult assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("bc7e2b0c-96aa-4a87-b1bf-23cef2a835e1"); }
        }
    }
}