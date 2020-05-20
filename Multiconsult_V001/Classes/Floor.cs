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
        public Brep[] surface;
        public Point3d[] nodes;
        public Curve boundaryExternal;
        public Curve[] boundaryInternal;
        public Plane plane;
        public string name;
        public int id;
        public Floor_Section section;
        public Material material;
        public Brep brep;

        //constructors
        public Floor()
        {
            //empty constructor
        }
        public Floor(Brep[] _surface)
        {
            surface = _surface;
        }

        public Floor(int _id, Brep[] _surface)
        {
            id = _id;
            surface = _surface;
        }
    }
}
