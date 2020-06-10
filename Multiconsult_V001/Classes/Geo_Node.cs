using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    public class Geo_Node
    {
        public int id;
        public string name1;
        public string name2;
        public Point3d point;
        public Geo_Surface geo_Srf1;
        public Geo_Surface geo_Srf2;
        public Geo_Layer geo_Layer1;
        public Geo_Layer geo_Layer2;

        public Geo_Node()
        { 
            //empty constructor
        }

        public Geo_Node(int _id,  Point3d _pt)
        {
            id = _id;
            point = _pt;
        }
    }
}
