using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Floor_Section
    {
        //properties
        public string name;
        public int id;
        public double width;

        //constructors
        public Floor_Section()
        {
            //empty constructor
        }

        public Floor_Section(string _name, double _width)
        {
            name = _name;
            width = _width;
        }
    }
}
