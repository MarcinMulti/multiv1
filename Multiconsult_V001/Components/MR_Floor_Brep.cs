using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;

namespace Multiconsult_V001.Components
{
    public class MR_Floor_Brep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MR_Floor_Brep class.
        /// </summary>
        public MR_Floor_Brep()
          : base("FloorBrep", "FLb",
              "Make a multiconsult floor from brep",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "BC", "Brep which have to be change to column", GH_ParamAccess.item);
            pManager.AddTextParameter("RevitMaterials", "RM", "Revit element materials", GH_ParamAccess.item,"Revit Material : Betong - B35");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiFloor", "MF", "Mulitconsult floor object", GH_ParamAccess.item);
            pManager.AddTextParameter("Informations", "I", "Informations about transition", GH_ParamAccess.list);
            pManager.AddBrepParameter("Surface", "S", "MasterSurface", GH_ParamAccess.list);
            pManager.AddCurveParameter("Boundaries", "b", "Curves boundaries", GH_ParamAccess.list);
            pManager.AddCurveParameter("InternalBoundaries", "ib", "Curves boundaries", GH_ParamAccess.list);
            pManager.AddCurveParameter("ExternalBoundaries", "eb", "Curves boundaries", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Brep brep = new Brep();
            string type = "";

            DA.GetData(0, ref brep);
            DA.GetData(1, ref type);

            //parameters
            List<string> infos = new List<string>();

            //create Multiconsut column
            infos.Add(" Create Mulitconsult column class ");
            var cpts = Methods.Geometry.findBottomAndTopCenterPoints(brep);
            //create Multiconsult columne object
            Floor f = new Floor();

            //assign section to column
            f.section = Methods.Geometry.findFloorSection(brep);

            //get materials from revit string
            string[] RevitMats = type.Split(':');
            Material m = new Material();
            m.RevitMaterialName = RevitMats[1];
            string[] matName = type.Split('-');
            m.name = matName[1].Trim();

            //assign material to column
            f.material = m;
            f.brep = brep;

            //code for finding basic geometry
            List<Curve> ibcrvs = new List<Curve>(); //internal boundaries

            //find central plane
            f.plane = new Plane(new Line(cpts[0], cpts[1]).PointAt(0.5), new Vector3d(0, 0, 1)); //XY plane in the middle of the floor height
            
            //intersect the brep with the plane to find master surface and boundaries
            Curve[] icrvs;
            Point3d[] ipts;
            var didItSect = Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, f.plane, 0.00001, out icrvs, out ipts );
            ibcrvs = icrvs.ToList();
            ibcrvs.RemoveAt(0);
            f.boundaryExternal = icrvs[0];
            f.boundaryInternal = ibcrvs.ToArray();
            f.nodes = ipts;
            
            var b = Brep.CreatePlanarBreps(icrvs, 0.000001);
            f.surface = b;

            //validate the information about analysis
            infos.Add("Section appears? = " + didItSect);
            infos.Add("Revit Material= " + m.RevitMaterialName + " MultiMaterial name= " + m.name);
            infos.Add("Section = " + f.section.name + " with thickness= " + f.section.width);

            //outputs
            DA.SetData(0, f);
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
            get { return new Guid("a6bdfaf0-bc39-4419-8365-394e664a3d05"); }
        }
    }
}