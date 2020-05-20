using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multiconsult_V001.Classes;
using Rhino;
using Rhino.Geometry;

namespace Multiconsult_V001.Classes
{
    class Wall_Section
    {
        //properties
        public string name;
        public int id;
        public Plane plane;
        public double width;

        //constructors
        public Wall_Section()
        {
            //empty
        }

        public Wall_Section(string _name, double _width)
        {
            name = _name;
            width = _width;
        }
    }
}
