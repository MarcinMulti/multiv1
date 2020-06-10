using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Multiconsult_V001.Properties;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;


namespace Multiconsult_V001.FEMDesign
{
    public class MF_FEMDesignModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MF_FEMDesignModel class.
        /// </summary>
        public MF_FEMDesignModel()
          : base("FEMDesign", "FEMD",
              "Prepare FEM design model",
              "Multiconsult", "FEMDesign")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
            pManager.AddNumberParameter("scaleFactor", "SF", "scaling factor, FEM Design is working in meters...", GH_ParamAccess.item, 1);
            pManager.AddPointParameter("ModelToPoint", "MM", "Translate the bottom of the model to this specific Point",GH_ParamAccess.item, new Point3d(0,0,0));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FEMDesignModel", "MA", "Multiconsult assembly", GH_ParamAccess.item);
            //columns
            pManager.AddCurveParameter("ColumnLines", "CL", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("ColumnMaterial", "CM", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("ColumnSection", "CS", "Multiconsult assembly", GH_ParamAccess.list);

            //walls
            pManager.AddCurveParameter("WallCurve", "WC", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("WallMaterial", "WM", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("WallSection", "WT", "Multiconsult assembly", GH_ParamAccess.list);

            //floors
            pManager.AddBrepParameter("SlabSurface", "SS", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("SlabMaterial", "SM", "Multiconsult assembly", GH_ParamAccess.list);
            pManager.AddTextParameter("SlabSection", "ST", "Multiconsult assembly", GH_ParamAccess.list);

            pManager.AddVectorParameter("MVector", "vec", "translation vector", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input
            Assembly model = new Assembly();
            double scalef = 1;
            Point3d targetPlan = new Point3d();

            DA.GetData(0, ref model);
            DA.GetData(1, ref scalef);
            DA.GetData(2, ref targetPlan);

            //variables
            Assembly newmodel = new Assembly();
            
            List<NurbsCurve> columnAxes = new List<NurbsCurve>();
            List<string> columnMaterials = new List<string>();
            List<string> columnSections = new List<string>();

            List<PolylineCurve> wallCurves = new List<PolylineCurve>();
            List<string> wallMaterials = new List<string>();
            List<string> wallSection = new List<string>();

            List<Brep> slabSurface = new List<Brep>();
            List<string> slabMaterials = new List<string>();
            List<string> slabSection = new List<string>();

            model.calculateBB();
            Point3d sourcePlan = model.bb.ToBrep().Vertices[0].Location;
            Vector3d moveVec = new Vector3d(
                targetPlan.X - sourcePlan.X, 
                targetPlan.Y - sourcePlan.Y, 
                targetPlan.Z - sourcePlan.Z);

            Rhino.Geometry.Transform tscale = Rhino.Geometry.Transform.Scale(sourcePlan, scalef);
            Rhino.Geometry.Transform tmove = Rhino.Geometry.Transform.Translation(moveVec);
            Rhino.Geometry.Transform comp = tmove*tscale;

            //methods
            foreach (var c in model.columns)
            {
                Rhino.Geometry.Line columnLine = c.Value.line;
                NurbsCurve columnCurve = columnLine.ToNurbsCurve();
                
                columnCurve.Transform(tmove);
                columnCurve.Scale(scalef);

                columnAxes.Add(columnCurve);


                columnMaterials.Add(c.Value.material.name);

                //create FEM design section name
                string FEMDesignName = "Steel sections";

                if (c.Value.material.name[0] == 'B')
                    FEMDesignName = "Concrete sections";
                else if(c.Value.material.name[0] == 'C')
                    FEMDesignName = "Timber sections";

                if (c.Value.section.type ==0)
                { 
                    FEMDesignName = FEMDesignName + ", Circle";
                    FEMDesignName = FEMDesignName + ", D " +c.Value.section.dim1;
                }

                if (c.Value.section.type == 1)
                { 
                    FEMDesignName = FEMDesignName + ", Square";
                    FEMDesignName = FEMDesignName + ", " + c.Value.section.dim1;
                }

                if (c.Value.section.type == 2)
                { 
                    FEMDesignName = FEMDesignName + ", Rectangle";
                    FEMDesignName = FEMDesignName + ", " + c.Value.section.dim1 + "x" + c.Value.section.dim2;
                }




                columnSections.Add(FEMDesignName);
            }

            foreach (var w in model.walls)
            {
                Point3d p1 = w.Value.bottomAxis.PointAtStart;
                Point3d p2 = w.Value.bottomAxis.PointAtEnd;
                Point3d p3 = w.Value.topAxis.PointAtEnd;
                Point3d p4 = w.Value.topAxis.PointAtStart;
                
                p1.Transform(tmove);
                p2.Transform(tmove);
                p3.Transform(tmove);
                p4.Transform(tmove);
                
                var pl = new Polyline(
                    new List<Point3d>() 
                    { p1, p2, p3, p4, p1 }
                    );

                PolylineCurve pc = pl.ToPolylineCurve();
                pc.Scale(scalef);
                //pc.Transform(tmove);
                wallCurves.Add(pc) ;
                wallMaterials.Add(w.Value.material.name);
                double wallThickness = w.Value.section.width * scalef;
                wallSection.Add( wallThickness.ToString());
            }

            foreach (var f in model.floors)
            {
                Brep floorSrf = f.Value.surface[0];
                floorSrf.Transform(comp);
                
                slabSurface.Add(floorSrf);
                slabMaterials.Add(f.Value.material.name);

                double slabThickness = f.Value.section.width*scalef;

                slabSection.Add( slabThickness.ToString());
            }

            //output
            DA.SetData(0, newmodel);
            DA.SetDataList(1, columnAxes);
            DA.SetDataList(2, columnMaterials);
            DA.SetDataList(3, columnSections);

            DA.SetDataList(4, wallCurves);
            DA.SetDataList(5, wallMaterials);
            DA.SetDataList(6, wallSection);

            DA.SetDataList(7, slabSurface);
            DA.SetDataList(8, slabMaterials);
            DA.SetDataList(9, slabSection);

            DA.SetData(10, moveVec);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.iconF;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("73d76292-79d7-4089-b11f-2dfbe9142d0d"); }
        }
    }
}