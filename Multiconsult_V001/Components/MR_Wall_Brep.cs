using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Components
{
    public class MR_Wall_Brep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MR_Wall_Brep()
          : base("WallFromBrep", "WA",
              "Create Multiconsult Wall object from brep",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep with the wall", GH_ParamAccess.item);
            pManager.AddTextParameter("RevitMaterials", "RM", "Revit Material from revit element", GH_ParamAccess.item,"Revit Material : Betong - B35");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Multiwall", "MW", "Mulitconsult wall object", GH_ParamAccess.item);
            pManager.AddTextParameter("Informations", "I", "Informations about transition", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Brep brep = new Brep();
            string type = "";
            DA.GetData(0, ref brep);
            DA.GetData(1, ref type);

            //parameters
            Wall w = new Wall("straight wall");

            //variables
            List<string> infos = new List<string>();
            List<Brep> srfs = new List<Brep>();
            List<Curve> ibcrvs = new List<Curve>(); //internal boundaries
            List<Curve> bcvs = new List<Curve>(); //bottom boundaries
            List<Curve> tcvs = new List<Curve>(); //top boundaries
            //methods
            infos.Add("Creting Multiconsult wall");
            w.brep = brep;
            var cpts = Methods.Geometry.findBottomAndTopCenterPoints(brep);  //find bottom and top central points
            var pls = Methods.Geometry.findWallHorizontalPlanes(brep);
            var crvs = Methods.Geometry.findWallBottomAndTop(brep);
            //find central plane
            w.planeHorizontal = pls[0]; //XY plane in the middle of the wall height
            //find bottom and top planes
            w.planeBottom = pls[1];
            w.planeTop = pls[2];

            //create master surface and bottom and top curves
            srfs.Add(Brep.CreatePlanarBreps(crvs[0], 0.00001)[0]);
            bcvs.Add(crvs[1]);
            //tcvs.Add(crvs[2]);

            //find the longest curve and midle curve
            Line[] constructionLines = Methods.Geometry.findWallConstructionLines(crvs.ToArray(),cpts);
            w.constructionLines = constructionLines;

            Line heightLine = new Line(cpts[0],cpts[1]);
            NurbsCurve bottomAxisCurve = constructionLines[0].ToNurbsCurve();
            var projectToBottom = Transform.PlanarProjection(w.planeBottom);
            bottomAxisCurve.Transform(projectToBottom);

            Point3d p1 = new Point3d(constructionLines[0].FromX, constructionLines[0].FromY, constructionLines[2].FromZ);
            Point3d p2 = new Point3d(constructionLines[0].ToX, constructionLines[0].ToY, constructionLines[2].FromZ);
            Point3d p3 = new Point3d(constructionLines[0].FromX, constructionLines[0].FromY, constructionLines[2].ToZ);
            Point3d p4 = new Point3d(constructionLines[0].ToX, constructionLines[0].ToY, constructionLines[2].ToZ);

            w.bottomAxis = new Line(p1, p2).ToNurbsCurve();
            w.topAxis = new Line(p3, p4).ToNurbsCurve();

            Point3d wallOrigin = constructionLines[2].PointAt(0.5);
            Vector3d wallX = Point3d.Subtract(constructionLines[0].From, constructionLines[0].To);
            Vector3d wallY = Point3d.Subtract(constructionLines[2].From, constructionLines[2].To);
            w.plane = new Plane(wallOrigin, wallX, wallY);

            //assign section
            w.section = new Wall_Section( "straight wall", Math.Round(constructionLines[1].Length));
            //assign material
            w.material = Methods.Revit.getRevitMaterialFromString(type);

            Brep masterSurface = Brep.CreateFromSweep(w.constructionLines[2].ToNurbsCurve(), w.bottomAxis, true, 0.000001)[0];
            w.surface = masterSurface;

            //validate the information about analysis
            infos.Add("Revit Material= " + w.material.RevitMaterialName);
            infos.Add("Section = " + w.section.name);

            //outputs
            DA.SetData(0, w);
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
            get { return new Guid("2402784f-7b7b-4ad5-ba12-9849549dde7d"); }
        }
    }
}