using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Multiconsult_V001.Classes;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Linq;

namespace Multiconsult_V001.Plaxis
{
    public class MP_BuildSoil : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetSoilCharacteristic class.
        /// </summary>
        public MP_BuildSoil()
          : base("BuildSoilData", "BSD",
              "Create GeoSurface data",
              "Multiconsult", "Plaxis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Terrain", "T", "Terrain surface", GH_ParamAccess.item);
            pManager.AddGenericParameter("Boreholes","B","List of boreholes",GH_ParamAccess.list);
            pManager.AddCurveParameter("Boundary","B","Boundary curve",GH_ParamAccess.item);
            pManager.AddNumberParameter("prec","p","number of points in 1 direction",GH_ParamAccess.item,10);
            pManager.AddNumberParameter("tole", "t", "number of points in 2 direction", GH_ParamAccess.item,0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Soil","S","Geo_soil", GH_ParamAccess.item);
            pManager.AddGenericParameter("Geosurface", "gS", "Geo_surface", GH_ParamAccess.list);
            pManager.AddGenericParameter("Boreholes", "bH", "Geo_boreholes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Geo_Terrain gt = new Geo_Terrain();
            List<Geo_Borehole> gbs = new List<Geo_Borehole>();
            Curve crv = new Line().ToNurbsCurve();
            double prec = 10;
            double tole = 10;

            //input
            DA.GetData(0, ref gt);
            DA.GetDataList(1, gbs);
            DA.GetData(2, ref crv);
            DA.GetData(3, ref prec);
            DA.GetData(4, ref tole);

            //variables
            List<Point3d> pts = new List<Point3d>();
            List<Brep> srfs = new List<Brep>();
            List<string> infos = new List<string>();
            List<Mesh> meshes = new List<Mesh>();
             
            Polyline pl = new Polyline();
            crv.TryGetPolyline(out pl);

            List<Point3d> gtPts = Point3d.CullDuplicates(gt.points, tole).ToList();
            //create flat grid point for terrain
            var tfpts = Methods.Plaxis.createGridOfFlatPoints(pl, prec);
            var tpts = Methods.Plaxis.createGridOfSpatialPoints(tfpts, gt.points);
            
            //divide region into grid of defined span
            int n1 = Convert.ToInt32(new Line(pl[0], pl[1]).Length / prec);
            int n2 = Convert.ToInt32(new Line(pl[1], pl[2]).Length / prec);

            var tmesh = Methods.Plaxis.createRectangleMesh(tpts,n2, n1);
            NurbsSurface nS = NurbsSurface.CreateFromPoints(tpts, n1 + 1, n2 + 1, 2, 2);
            Brep bS = nS.ToBrep();
            Geo_Surface gSt = new Geo_Surface();

            meshes.Add( tmesh );
            pts.AddRange( tpts );
            srfs.Add( nS.ToBrep() );
            gSt.name = "Terrain";
            gSt.points = tpts.ToArray();
            gSt.surface = nS.ToBrep();
            
            //correct geoterrain
            gt.surface = nS;

            //correct boreholes
            List<Geo_Borehole> gbhs = new List<Geo_Borehole>();
            foreach (var boreh in gbs)
            {
                Geo_Borehole gbh = new Geo_Borehole(boreh.id);
                List<Geo_Node> nodes = new List<Geo_Node>();

                gbh.name = boreh.name;
                gbh.position = boreh.position;
                //find geo node on the terrain surface
                double ub = 0;
                double vb = 0;
                nS.ClosestPoint( boreh.position, out ub, out vb );
                Geo_Node gn = new Geo_Node();
                gn.name1 = "Terrain";
                gn.name2 = "Terrain";
                gn.point = nS.PointAt(ub,vb);
                gn.geo_Srf1 = gSt;
                gn.geo_Srf2 = gSt;

                nodes.Add(gn);
                nodes.Add(boreh.nodes[0]);

                for (int ib = 0; ib < boreh.nodes.Count - 1; ib++)
                {
                    if (boreh.nodes[ib].point.DistanceTo(boreh.nodes[ib + 1].point) > 0.001)
                    {
                        nodes.Add(boreh.nodes[ib + 1]);
                    }
                }
                var snodes = nodes.OrderBy(item => item.point.Z).ToList();
                snodes.Reverse();
                gbh.nodes = snodes;
                gbhs.Add(gbh);
            }

            //create geo surfaces
            var allGeoNodes = gbhs.Select(item1 => item1.nodes).ToList();
            var geoNodes = allGeoNodes.SelectMany(item2 => item2).ToList();
            var allLayerNames = geoNodes.Select(item3 => item3.name2).ToList();
            var uniqueLayerNames = allLayerNames.Distinct();
            infos.AddRange(uniqueLayerNames);

            List<Geo_Surface> gsfs = new List<Geo_Surface>();
            foreach (var uLN in uniqueLayerNames)
            {
                Geo_Surface gsf = new Geo_Surface();
                gsf.name = uLN;
                List<Point3d> pts1 = new List<Point3d>();
                List<Geo_Node> nodes1 = new List<Geo_Node>();

                foreach (var gn in geoNodes)
                {
                    if (gn.name2 == uLN)
                    {
                        nodes1.Add(gn);
                        pts1.Add(gn.point);        
                    }
                }

                gsf.nodes = nodes1;
                gsf.points = pts1.ToArray();

                //create flat grid point for terrain
                List<Point3d> gPts = Point3d.CullDuplicates(pts1, tole).ToList();
                var gfpts = Methods.Plaxis.createGridOfFlatPoints(pl, prec);
                var gpts = Methods.Plaxis.createGridOfSpatialPoints(gfpts, gPts);
                
                //divide region into grid of defined span
                int gn1 = Convert.ToInt32(new Line(pl[0], pl[1]).Length / prec);
                int gn2 = Convert.ToInt32(new Line(pl[1], pl[2]).Length / prec);

                NurbsSurface gnS = NurbsSurface.CreateFromPoints(gpts, gn1 + 1, gn2 + 1, 2, 2);
                pts.AddRange(gpts);
                var gmesh = Methods.Plaxis.createRectangleMesh(gpts, gn2, gn1 );
                Brep gbS = gnS.ToBrep();
                srfs.Add(gbS);
                meshes.Add(gmesh);

                gsf.points = gpts.ToArray();
                gsf.surface = gbS;
                gsf.mesh = gmesh;
                gsfs.Add(gsf);
            }

            List<System.Drawing.Color> cls = new List<System.Drawing.Color>();
            int igc = 0;
            int ibc = 0;
            int irc = 0;
            int change = Convert.ToInt32((3*255)/srfs.Count());

            //build soils
            Geo_Soil soilModel = new Geo_Soil();
            soilModel.name = "Soil model without layers";
            soilModel.id = 0;
            soilModel.terrain = gt;
            soilModel.geosurfaces = gsfs;
            soilModel.boreholes = gbhs;

            //output
            DA.SetData(0, soilModel);
            DA.SetDataList(1, gsfs);
            DA.SetDataList(2, gbhs);
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
            get { return new Guid("ae3a9bd0-6937-4332-aaec-050c50072186"); }
        }
    }
}