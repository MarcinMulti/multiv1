using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;


namespace Multiconsult_V001.deconstructors
{
    public class DeconstructFloor : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructFloor()
           : base("DeconstructFloor", "DF",
              "Deconstruct the floor properties",
              "Multiconsult", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiFloor", "MF", "Multiconsult Floor object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "Srf", "Master Surface", GH_ParamAccess.list); //0
            pManager.AddCurveParameter("ExternalCrv", "Ecrv", "External boundary curve", GH_ParamAccess.item); //1
            pManager.AddCurveParameter("InternalCrv", "Icrv", "Intrnals curves", GH_ParamAccess.list); //2
            pManager.AddPlaneParameter("Plan", "Pl", "Master plane of the floor", GH_ParamAccess.item); //3
            pManager.AddBrepParameter("Brep", "B", "Brep representing floor", GH_ParamAccess.item); //4
            //pManager.AddLineParameter("ConstrLines", "CLs", "Construction lines", GH_ParamAccess.list); //5
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Floor f = new Floor();
            DA.GetData(0, ref f);

            DA.SetDataList(0, f.surface); //0
            DA.SetData(1, f.boundaryExternal); //1
            DA.SetDataList(2, f.boundaryInternal); //2
            DA.SetData(3, f.plane); //3
            DA.SetData(4, f.brep); //4
            //DA.SetDataList(5, f.); //5
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
            get { return new Guid("0a39b8ce-a4a0-464f-8c9b-fcb6117a1125"); }
        }
    }
}