using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Components
{
    public class MR_Walls : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MR_Walls()
          : base("Wall", "WA",
              "Description",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface of the wall", GH_ParamAccess.list);
            pManager.AddTextParameter("Section", "S", "Section of the column", GH_ParamAccess.list);
            pManager.AddTextParameter("Material", "M", "Material of the column", GH_ParamAccess.list, new List<string>() { "C20/25" });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Multiwall", "MW", "Mulitconsult wall object", GH_ParamAccess.list);
            pManager.AddTextParameter("Informations", "I", "Informations about transition", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<Surface> srfs = new List<Surface>();
            List<string> sects = new List<string>();
            List<string> mats = new List<string>();
            DA.GetDataList(0, srfs);
            DA.GetDataList(1, sects);
            DA.GetDataList(2, mats);

            //parameters
            List<Wall> wls = new List<Wall>();
            List<string> infos = new List<string>();

            int nwls = srfs.Count;
            int nsects = sects.Count;

            infos.Add("number of surfaces=" + nwls + "number of sections=" + nsects);

            if (nwls != nsects)
            {
                infos.Add("number of surfaces and sections have to be the same");
                var ma = new GH_RuntimeMessage("The lines and description number is not the same, check it out", GH_RuntimeMessageLevel.Error, null);
            }
            else
            {
                infos.Add("The process of creating walls started");
                for (int i = 0; i < nwls; i++)
                {
                    var wl = new Wall(-1, srfs[i]);
                    wl.name = "flat floor";
                    wl.section = sects[i];
                    wl.material = mats[0];
                    wls.Add(wl);
                }
            }

            DA.SetDataList(0, wls);
            DA.SetDataList(1, infos);
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
            get { return new Guid("410051ca-4b42-4abb-ad28-f51db1f24a1d"); }
        }
    }
}