using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.deconstructors
{
    public class DeconstructAssembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructAssembly class.
        /// </summary>
        public DeconstructAssembly()
          : base("DeconstructAssembly", "DA",
              "Deconstruct assembly properties",
              "Multiconsult", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly","MA","Multiconsult assembly",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("AllBreps", "MBs", "All REvitBreps in the model", GH_ParamAccess.list);
            pManager.AddGenericParameter("MultiColumns", "MC", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddGenericParameter("MultiWalls", "MW", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddGenericParameter("MultiFloors", "MF", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddLineParameter("LineColumns", "LC", "All column lines", GH_ParamAccess.list);
            pManager.AddBrepParameter("SurfaceWalls", "SW", "All surface walls", GH_ParamAccess.list);
            pManager.AddBrepParameter("SurfaceFloors", "SF", "All surface floors", GH_ParamAccess.list);
            pManager.AddBrepParameter("BB", "BB", "Bounding box", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Assembly model = new Assembly();

            DA.GetData(0, ref model);

            List<Brep> bs = new List<Brep>();
            List<Line> ls = new List<Line>();
            List<Brep> ws = new List<Brep>();
            List<Brep> fs = new List<Brep>();

            foreach (var c in model.columns)
            {
                bs.Add(c.Value.brep);
                ls.Add(c.Value.line);
            }

            foreach (var w in model.walls)
            {
                bs.Add(w.Value.brep);
                ws.Add(w.Value.surface);
            }

            foreach (var f in model.floors)
            {
                bs.Add(f.Value.brep);
                fs.AddRange(f.Value.surface);
            }
            model.calculateBB();

            DA.SetDataList(0, bs);
            DA.SetDataList(1, model.columns.Values);
            DA.SetDataList(2, model.walls.Values);
            DA.SetDataList(3, model.floors.Values);
            DA.SetDataList(4, ls);
            DA.SetDataList(5, ws);
            DA.SetDataList(6, fs);
            DA.SetData(7, model.bb.ToBrep());
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
            get { return new Guid("35ac1ba7-2f06-45f5-9742-264551c72277"); }
        }
    }
}