using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.deconstructors
{
    public class DeconstructWall : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructWall class.
        /// </summary>
        public DeconstructWall()
          : base("DeconstructWall", "DW",
              "Deconstruct the wall properties",
              "Multiconsult", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiWall","MW","Multiconsult Wall object",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "Srf", "Master Surface", GH_ParamAccess.item); //0
            pManager.AddCurveParameter("Axis", "A", "Master Surface axis on the bottom", GH_ParamAccess.item); //1
            pManager.AddPlaneParameter("Plane", "Pl", "WallPlane", GH_ParamAccess.item); //2
            pManager.AddPlaneParameter("BottomPlane", "BPl", "Master Surface bottom plane", GH_ParamAccess.item); //3
            pManager.AddPlaneParameter("TopPlane", "TPl", "Master Surface top plane", GH_ParamAccess.item); //4
            pManager.AddLineParameter("ConstrLines", "CLs", "Construction lines", GH_ParamAccess.list); //5
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Wall w = new Wall();
            DA.GetData(0, ref w);

            DA.SetData(0, w.surface); //0
            DA.SetData(1, w.bottomAxis); //1
            DA.SetData(2, w.plane); //2
            DA.SetData(3, w.planeBottom); //3
            DA.SetData(4, w.planeTop); //4
            DA.SetDataList(5, w.constructionLines.ToList() ); //4
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
            get { return new Guid("ca5dab7c-3bd4-4571-96d9-05f0faa2743f"); }
        }
    }
}