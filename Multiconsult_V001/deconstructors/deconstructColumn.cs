using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.deconstructors
{
    public class DeconstructColumn : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the deconstructColumn class.
        /// </summary>
        public DeconstructColumn()
          : base("DeconstructBar", "DB",
              "Deconstruct the column,beam,bar properties",
              "Multiconsult", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar", "S", "Bar section, beam section, column section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of the section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Section", "S", "Width of the section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "Width of the section", GH_ParamAccess.item);
            pManager.AddCurveParameter("Axis", "A", "Height of the section", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Column c = new Column();

            DA.GetData(0, ref c);

            DA.SetData(0, c.name);
            DA.SetData(1, c.section);
            DA.SetData(2, c.material);
            DA.SetData(3, c.line);
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
            get { return new Guid("3e2ca324-deed-4640-b77c-c5f5ee2df3b9"); }
        }
    }
}