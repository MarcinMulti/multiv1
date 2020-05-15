using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Geo_Node
    {
        public int id;
        public string name;
        public Point3d point;

        public Geo_Node()
        { 
            //empty constructor
        }

        public Geo_Node(int _id, string _name, Point3d _pt)
        {
            id = _id;
            name = _name;
            point = _pt;
        }
    }
}
