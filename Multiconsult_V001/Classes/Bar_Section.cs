using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Multiconsult_V001.Classes
{
    class Bar_Section
    {
        //properties
        public string name;
        public int id;
        public Point3d centerPoint;
        public double area;
        public Vector3d vec1;
        public Vector3d vec2;
        public double dim1;
        public double dim2;
        public Curve[] edges;
        public int type;

        public string material;

        //constructors
        public Bar_Section()
        {
            //empty constructor
        }

        public Bar_Section(string _name)
        {
            name = _name;

        }

        public Bar_Section(int _id, string _name)
        {
            id = _id;
            name = _name;
        }
    }
}
