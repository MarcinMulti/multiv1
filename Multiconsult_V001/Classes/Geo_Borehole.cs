using GH_IO.Serialization;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    public class Geo_Borehole
    {
        public string name;
        public int id;
        public Point3d[] points;
        public Point3d position;
        public List<Geo_Node> nodes;
        public Point3d topPoint;

        public Geo_Borehole()
        { 
            //empty constructor
        }

        public Geo_Borehole(int _id)
        {
            id = _id;
        }

        void sortNodes()
        {
           nodes = nodes.OrderBy(item => item.point.Z).ToList();
        }
    }
}
