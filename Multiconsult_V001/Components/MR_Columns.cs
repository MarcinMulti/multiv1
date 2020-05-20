using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Multiconsult_V001
{
    public class MR_Columns : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MR_Columns()
          : base("Column", "CO",
              "Description",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Axis of the column", GH_ParamAccess.list);
            pManager.AddTextParameter("Section", "S", "Section of the column", GH_ParamAccess.list);
            pManager.AddTextParameter("Material", "M", "Material of the column", GH_ParamAccess.list, new List<string>() { "C20/25"});
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiColumn", "MC", "Mulitconsult column object", GH_ParamAccess.list);
            pManager.AddTextParameter("Informations","I","Informations about transition",GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<Line> lines = new List<Line>();
            List<string> sects = new List<string>();
            List<string> mats = new List<string>();
            List<Curve> crvsecs = new List<Curve>();

            DA.GetDataList(0, lines);
            DA.GetDataList(1, sects);
            DA.GetDataList(2, mats);
            DA.GetDataList(3, crvsecs);

            //parameters
            List<Column> cols = new List<Column>();
            List<string> infos = new List<string>();

            int nlines = lines.Count;
            int nsects = sects.Count;

            infos.Add("number of line=" + nlines + "number of sections=" + nsects);

            if (nlines != nsects)
            {
                infos.Add("number of lines and sections have to be the same");
                var ma = new GH_RuntimeMessage("The lines and description number is not the same, check it out", GH_RuntimeMessageLevel.Error, null);
            }
            else 
            {
                infos.Add("The process of creating columns started");
                for (int i = 0; i < nlines; i++)
                {
                    
                    var col = new Column(-1,lines[i]);
                    col.name = "straight column";
                    cols.Add(col);
                    
                    //infos.Add("column number=" + i + " type=" + col.name);
                    //infos.Add("column id=" + col.id + " material=" + col.material);
                }
            }    

            DA.SetDataList(0, cols);
            DA.SetDataList(1, infos);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f8611e00-e5cb-4fd0-9676-9ca0a28bb10c"); }
        }
    }
}
