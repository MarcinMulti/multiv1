using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Multiconsult_V001.Classes;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;


namespace Multiconsult_V001.Components
{
    public class MC_CorrectModel_2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_CorrectModel_2 class.
        /// </summary>
       public MC_CorrectModel_2()
          : base("CorrectModel_2", "cor",
              "Correct all elements rounding of the model ",
              "Multiconsult", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumberOfDigits", "ND", "Number of digits to round", GH_ParamAccess.item, 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MultiAssembly", "MA", "Multiconsult assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input
            Assembly model = new Assembly();
            int digits = 5;

            DA.GetData(0, ref model);
            DA.GetData(1, ref digits);
            
            Assembly newmodel = new Assembly();

            var cols = model.columns;
            var flos = model.floors;
            var wals = model.walls;

            foreach (var c in cols.Values)
            {
                c.pt_end = roundPoint(c.pt_end, digits);

                c.pt_st = roundPoint(c.pt_st, digits);

                c.line = new Line(c.pt_st, c.pt_end);
            }

            foreach (var f in flos.Values)
            {
                Polyline ple = new Polyline();
                f.boundaryExternal.TryGetPolyline(out ple);
                List<Point3d> nptse = new List<Point3d>();
                foreach (var p in ple)
                {
                    nptse.Add(roundPoint(p,digits));
                }
                f.boundaryExternal = new Polyline(nptse).ToPolylineCurve();


                if (f.boundaryInternal.Length > 0)
                {
                    List<Curve> allCurves = new List<Curve>();
                    List<Curve> internalCurves = new List<Curve>();
                    foreach (var bi in f.boundaryInternal)
                    {
                        Polyline pli = new Polyline();
                        bi.TryGetPolyline(out pli);
                        List<Point3d> nptsi = new List<Point3d>();
                        foreach (var pi in pli)
                        {
                            nptsi.Add(roundPoint(pi, digits));
                        }
                        internalCurves.Add(new Polyline(nptsi).ToPolylineCurve());
                    }
                    f.boundaryInternal = internalCurves.ToArray();
                    allCurves.Add(f.boundaryExternal);
                    allCurves.AddRange(internalCurves);

                    f.surface = Brep.CreatePlanarBreps(allCurves, 0.0001);
                }
                else
                {
                    f.surface = Brep.CreatePlanarBreps( f.boundaryExternal, 0.0001);
                }

            }
            flos.OrderBy(fz => fz.Value.plane.OriginZ);

            List<int> wallsToRemove = new List<int>();
            foreach (var w in wals)
            {
                Line ba = new Line(w.Value.bottomAxis.PointAtStart, w.Value.bottomAxis.PointAtEnd);
                Line ta = new Line(w.Value.topAxis.PointAtStart, w.Value.topAxis.PointAtEnd);

                Point3d p1 = new Point3d(ba.FromX, ba.FromY, ba.FromZ);
                Point3d p2 = new Point3d(ba.ToX, ba.ToY, ba.ToZ);
                Point3d p3 = new Point3d(ba.FromX, ba.FromY, ta.FromZ);
                Point3d p4 = new Point3d(ba.ToX, ba.ToY, ta.ToZ);

                w.Value.bottomAxis = new Line(p1, p2).ToNurbsCurve();
                w.Value.topAxis = new Line(p3, p4).ToNurbsCurve();

                if (ba.Length < 10)
                    wallsToRemove.Add(w.Key);

                w.Value.makeMainSurfaceFromAxis();
            }

            foreach (var iwtr in wallsToRemove)
            {
                wals.Remove(iwtr);
            }

            newmodel.columns = cols;
            newmodel.walls = wals;
            newmodel.floors = flos;

            //output
            DA.SetData(0, newmodel);

        }

        public Point3d roundPoint(Point3d pt, int digits)
        {
            pt = new Point3d(
                    Math.Round(pt.X, digits),
                    Math.Round(pt.Y, digits),
                    Math.Round(pt.Z, digits)
                    );

            return pt;
        }

        public Line roundLine(Line l, int digits)
        {
            l = new Line(
                roundPoint(l.From, digits),
                roundPoint(l.To, digits)
                        );

            return l;
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
            get { return new Guid("317ad2dd-a87b-46ca-96d2-dd4e0080f315"); }
        }
    }
}