using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.deconstructors
{
    public class DeconstructSectionBar : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructorSectionBar class.
        /// </summary>
        public DeconstructSectionBar()
          : base("DeconstructBarSection", "DF",
              "Deconstruct the column,beam,bar section properties",
              "Multiconsult", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BarSection","S","Bar section, beam section, column section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name","N","Name of the section", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of the section", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of the section", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "N", "Type of the section", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bar_Section cs = new Bar_Section();

            DA.GetData(0, ref cs);

            DA.SetData(0, cs.name);
            DA.SetData(1, cs.dim1);
            DA.SetData(2, cs.dim2);
            DA.SetData(3, cs.type);

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
            get { return new Guid("979eea83-8968-4293-9321-bb88e5cf0bb1"); }
        }
    }
}