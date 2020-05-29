using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Components
{
    public class MR_Beam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_Beam class.
        /// </summary>
        public MR_Beam()
          : base("MC_Beam", "Nickname",
              "Description",
              "Multiconsult", "Revit")
        {
            
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Axis of the column", GH_ParamAccess.item);
            pManager.AddTextParameter("Section", "S", "Section of the column", GH_ParamAccess.item);
            pManager.AddTextParameter("Material", "M", "Material of the column", GH_ParamAccess.item,  "C20/25" );
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiColumn", "MC", "Mulitconsult column object", GH_ParamAccess.item);
            pManager.AddTextParameter("Informations", "I", "Informations about transition", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Line line = new Line();
            string sect = "section";
            string mat = "material";
            List<Curve> crvsecs = new List<Curve>();

            DA.GetData(0, ref line);
            DA.GetData(1, ref sect);
            DA.GetData(2, ref mat);

            //parameters
            List<string> infos = new List<string>();


            Beam b = new Beam(line);

            //assign section to column
            //b.section = ;
            b.name = "beam FromLine";
            b.pt_end = line.From;
            b.pt_st = line.To;
            infos.Add(b.name);
            //get materials from revit string
            string[] RevitMats = mat.Split(':');
            Material m = new Material();
            m.RevitMaterialName = RevitMats[1];
            string[] matName = mat.Split('-');
            m.name = matName[1].Trim();
            infos.Add(m.name);
            infos.Add(m.RevitMaterialName);

            //assign material to column
            b.material = m;
            DA.SetData(0, b);
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
            get { return new Guid("eb336345-8b10-4b79-a1b5-bc9d90d8498a"); }
        }
    }
}