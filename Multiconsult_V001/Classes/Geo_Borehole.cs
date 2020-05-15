using GH_IO.Serialization;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Geo_Borehole
    {
        public Point3d[] points;
        public Point3d position;
        public string name;
        public int id;

        public Geo_Borehole()
        { 
            //empty constructor
        }

        public Geo_Borehole(int _id)
        {
            id = _id;
        }
    }
}
