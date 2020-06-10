using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    public class Geo_Surface
    {
        public Brep surface;
        public Point3d[] points;
        public string name;
        public int id;
        public List<Geo_Node> nodes;
        public Mesh mesh;

        public Geo_Surface()
        { 
            //empty constructor
        }

    }
}
