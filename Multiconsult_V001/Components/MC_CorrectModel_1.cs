using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Multiconsult_V001.Components
{
    public class MC_CorrectModel_1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CorrectModel_1 class.
        /// </summary>
        public MC_CorrectModel_1()
          : base("CorrectModel_1", "cor",
              "Correct elements of the model ",
              "Multiconsult", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
            pManager.AddBooleanParameter("SplitColumns", "SC", "Split columns on the intersction with the floor plane", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("SplitWalls", "SW", "Split walls on the intersction with the floor plane", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("adjustWalls", "AW", "Adjust walls on the intersction with the floor plane", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Tolerances", "Ts", "Tolerances to adjust walls", GH_ParamAccess.list, new List<double>() { 500, 10, 400 });
            pManager.AddBooleanParameter("adjustFloors", "AF", "Adjust floors to walls", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
            pManager.AddGenericParameter("Outputs", "Out", "Multiconsult assembly", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input
            Assembly model = new Assembly();
            bool splitColumns = true;
            bool splitWalls = true;
            bool adjustWalls = true;
            List<double> tols = new List<double>();
            bool adjustFloors = true;

            DA.GetData(0, ref model);
            DA.GetData(1, ref splitColumns);
            DA.GetData(2, ref splitWalls);
            DA.GetData(3, ref splitWalls);
            DA.GetDataList(4, tols);
            DA.GetData(5, ref adjustFloors);

            Assembly newmodel = new Assembly();

            var cols = model.columns;
            var flos = model.floors;
            var wals = model.walls;

            //split the columns?
            if (splitColumns)
                 newmodel.columns = Multiconsult_V001.Methods.Geometry.createColumnsFromFloorPlanes(cols,flos);

            if (splitWalls)
                newmodel.walls = Multiconsult_V001.Methods.Geometry.createWallsFromFloorPlanes(wals, flos);

            if (adjustWalls)
            {
                double[] wallTolerances = tols.ToArray();
                newmodel.walls = Multiconsult_V001.Methods.Geometry.adjustWalls(newmodel.walls, wallTolerances);
                newmodel.floors = flos;

                //update walls
                foreach (Wall w in newmodel.walls.Values)
                {
                    w.makeMainSurfaceFromAxis();
                    w.refreshConstructionLineFromAxis();
                }
            }

            if (adjustFloors)
            {
                
            }
            

            //output
            DA.SetData(0, newmodel);
            DA.SetDataList(1, model.walls.Values.ToList() );
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
            get { return new Guid("9c5ba895-a13c-446c-84c0-3759f6251568"); }
        }
    }
}