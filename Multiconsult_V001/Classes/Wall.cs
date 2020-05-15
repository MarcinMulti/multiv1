using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Multiconsult_V001.Classes
{
    class Wall
    {
        //properties
        public Surface surface;
        public Point3d[] nodes;
        public Curve boundary;
        public Curve[] holes;
        public string name;
        public int id;
        public string section;
        public string material;

        //constructors
        public Wall(Surface _surface)
        {
            surface = _surface;
        }

        public Wall(int _id, Surface _surface)
        {
            id = _id;
            surface = _surface;
        }
    }
}
