using System;
using System.Collections.Generic;
using Multiconsult_V001.Classes;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace Multiconsult_V001.Components
{
    public class MR_Column_Brep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MR_Column_Brep class.
        /// </summary>
        public MR_Column_Brep()
          : base("ColumnFromBrep", "COB",
              "Coulmn from brep",
              "Multiconsult", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "bc", "Brep which have to be change to column", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "t", "Type of the element from the revit", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiColumn", "MC", "Mulitconsult column object", GH_ParamAccess.item);
            pManager.AddTextParameter("Informations", "I", "Informations about transition", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves1", "C", "Section Curves", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves2", "C", "Axis Curves", GH_ParamAccess.list);
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
            List<Column> cols = new List<Column>();
            List<string> infos = new List<string>();
            Curve crv = new Line().ToNurbsCurve();
            List<Curve> crvs = new List<Curve>();
            List<Brep> brs = new List<Brep>();

            infos.Add("This algorithm delivers the axis line, material and section curve/description of the column");

            Dictionary<int, double> dicZ = new Dictionary<int, double>();
            Dictionary<int, Surface> dicS = new Dictionary<int, Surface>();
            Dictionary<int, Point3d> dicP = new Dictionary<int, Point3d>();
            Dictionary<int, List<Curve>> dicC = new Dictionary<int, List<Curve>>();
            int iz = 0;
            foreach (var f in brep.Faces)
            {
                var ies = f.AdjacentEdges();
                List<Curve> ecrvs = new List<Curve>();

                foreach (var ie in ies)
                    ecrvs.Add(brep.Edges[ie]);
                
                Surface srf = f.ToNurbsSurface();
                Point3d center = AreaMassProperties.Compute(f.ToBrep()).Centroid;
                dicZ.Add(iz,center.Z);
                dicS.Add(iz, srf);
                dicP.Add(iz, center);
                dicC.Add(iz, ecrvs);
                iz++;
            }

            double minZ = dicZ.OrderBy(z => z.Value).First().Value;
            int iminZ = dicZ.OrderBy(z => z.Value).First().Key;
            double maxZ = dicZ.OrderBy(z => z.Value).Last().Value;
            int imaxZ = dicZ.OrderBy(z => z.Value).Last().Key;

            Surface bottomSrf = dicS[iminZ];
            Surface topSrf = dicS[imaxZ];
            Curve axis = new Line(dicP[iminZ],dicP[imaxZ]).ToNurbsCurve();
            crv = axis;
            var bottomCrvs = dicC[iminZ];
            var isarc = bottomCrvs[0].IsArc();
            Arc arc1 = new Arc();
            double radiusarc1 = 0;
            double width = 0;
            double height = 0;
            Vector3d vec1 = new Vector3d();
            Vector3d vec2 = new Vector3d();

            if (isarc)
            {
                //check if section is circular
                bottomCrvs[0].TryGetArc(out arc1);
                radiusarc1 = arc1.Radius;
                radiusarc1 = Math.Round(radiusarc1, 5);
                vec1 = Point3d.Subtract(arc1.Center, arc1.StartPoint);

                //create information set
                infos.Add("The section is circular");
                infos.Add("the radius is =");
                infos.Add(radiusarc1.ToString());
            }
            else
            {
                if (bottomCrvs.Count == 4)
                {
                    //check if section is rectangle or square
                    width = Math.Round(bottomCrvs[0].GetLength(), 5);
                    height = Math.Round(bottomCrvs[1].GetLength(), 5);

                    if (width == height)
                    {
                        //Square section
                        infos.Add("The section is square");
                        infos.Add("Dim1 =" + width);
                        infos.Add("Dim2 =" + height);

                        vec1 = Point3d.Subtract(dicP[iminZ],new Line(bottomCrvs[0].PointAtStart, bottomCrvs[0].PointAtEnd).PointAt(0.5));
                        vec2 = Point3d.Subtract(dicP[iminZ], new Line(bottomCrvs[1].PointAtStart, bottomCrvs[1].PointAtEnd).PointAt(0.5));
                    }
                    else
                    {
                        //Square section
                        infos.Add("The section is rectangle");
                        infos.Add("Dim1 =" + width);
                        infos.Add("Dim2 =" + height);

                        vec1 = Point3d.Subtract(dicP[iminZ], new Line(bottomCrvs[0].PointAtStart, bottomCrvs[0].PointAtEnd).PointAt(0.5));
                        vec2 = Point3d.Subtract(dicP[iminZ], new Line(bottomCrvs[1].PointAtStart, bottomCrvs[1].PointAtEnd).PointAt(0.5));
                    }
                    
                }
            }
            
            
            infos.Add("Z coordinate of bottom = " + minZ); 
            infos.Add("Z coordinate of top = " + maxZ);
            crvs.Add(new Line(dicP[iminZ],vec1).ToNurbsCurve());
            crvs.Add(new Line(dicP[iminZ], vec2).ToNurbsCurve());
            DA.SetDataList(0, cols);
            DA.SetDataList(1, infos);
            DA.SetData(2, crv);
            DA.SetDataList(3, crvs);
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
            get { return new Guid("a30ab3bd-46b1-4cc6-843e-87ce79ab2519"); }
        }
    }
}