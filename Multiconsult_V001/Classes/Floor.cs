using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Multiconsult_V001.Classes
{
    class Floor
    {
        //properties
        public Surface surface;
        public Point3d[] nodes;
        public Curve boundaryExternal;
        public Curve[] boundaryInternal;
        public Plane plane;
        public string name;
        public int id;
        public string section;
        public string material;

        //constructors
        public Floor(Surface _surface)
        {
            surface = _surface;
        }

        public Floor(int _id, Surface _surface)
        {
            id = _id;
            surface = _surface;
        }
    }
}
