using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Components
{
    public class MR_Assembler : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent3 class.
        /// </summary>
        public MR_Assembler()
          : base("Assembler", "AS",
              "Description",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiColumn", "MC", "Mulitconsult column object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Multifloor", "MF", "Mulitconsult floor object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Multiwall", "MW", "Mulitconsult wall object", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembler", "MA", "Mulitconsult assembled model object", GH_ParamAccess.item);
            pManager.AddGenericParameter("MultiColumn", "MC", "Mulitconsult column object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Multifloor", "MF", "Mulitconsult floor object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Multiwall", "MW", "Mulitconsult wall object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<Column> cols = new List<Column>();
            List<Wall> wls = new List<Wall>();
            List<Floor> fls = new List<Floor>();
            DA.GetDataList(0, cols);
            DA.GetDataList(2, wls);
            DA.GetDataList(1, fls);

            //methods
            Dictionary<int, Column> dcols = new Dictionary<int, Column>();
            Dictionary<int, Wall> dwls = new Dictionary<int, Wall>();
            Dictionary<int, Floor> dfls = new Dictionary<int, Floor>();

            //create dictionary of columns
            int ic = 0;
            foreach (var c in cols)
            {
                c.id = ic++;
                dcols.Add(c.id, c);
            }
            //create dicitionary of walls
            int iw = 0;
            foreach (var w in wls)
            {
                w.id = iw++;
                dwls.Add(w.id, w);
            }
            //create dictionary of floors
            int ifl = 0;
            foreach (var f in fls)
            {
                f.id = ifl++;
                dfls.Add(f.id, f);
            }

            //assign dictionaries to assembly
            Assembler assembly = new Assembler();
            assembly.columns = dcols;
            assembly.floors = dfls;
            assembly.walls = dwls;

            //outputs
            DA.SetData(0, assembly);
            DA.SetDataList(1, dcols.ToList());
            DA.SetDataList(2, dfls.ToList());
            DA.SetDataList(3, dwls.ToList());

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
            get { return new Guid("320ec9bd-00f7-4cd4-9e68-b5e9461c9b64"); }
        }
    }
}