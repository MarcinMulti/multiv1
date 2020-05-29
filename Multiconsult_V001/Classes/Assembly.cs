using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Assembly
    {
        public Dictionary<int, Column> columns;
        public Dictionary<int, Floor> floors;
        public Dictionary<int, Wall> walls;
        public Dictionary<int, Beam> beams;

        public BoundingBox bb;

        public Assembly()
        { 
            //empty constructor
        }

        public void calculateBB()
        {
            List<Point3d> pts = new List<Point3d>();
            if (columns != null)
            {
                foreach (var c in columns)
                {
                    pts.Add(c.Value.pt_end);
                    pts.Add(c.Value.pt_st);
                }
            }

            if (beams != null)
            {
                foreach (var b in beams)
                {
                    pts.Add(b.Value.pt_end);
                    pts.Add(b.Value.pt_st);
                }
            }

            if (floors != null)
            {
                foreach (var f in floors)
                {
                    Polyline pl = new Polyline();
                    f.Value.boundaryExternal.TryGetPolyline(out pl);
                    pts.AddRange(pl);
                }
            }

            if (walls != null)
            {
                foreach (var w in walls)
                {
                    Point3d p1 = w.Value.bottomAxis.PointAtStart;
                    Point3d p2 = w.Value.bottomAxis.PointAtEnd;
                    Point3d p3 = w.Value.topAxis.PointAtStart;
                    Point3d p4 = w.Value.topAxis.PointAtEnd;

                    pts.Add(p1);
                    pts.Add(p2);
                    pts.Add(p3);
                    pts.Add(p4);
                }
            }

            bb = new BoundingBox(pts);
        }

    }
}
