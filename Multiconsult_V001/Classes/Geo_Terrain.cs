using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Geo_Terrain
    {
        public List<Point3d> points;
        public string name;
        public Surface surface;
        public Mesh mesh;
        public Geo_Terrain()
        { 
            //empty constructor
        }

        public Geo_Terrain(List<Point3d> _pts)
        {
            points = _pts;
        }


    }
}
