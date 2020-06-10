﻿using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multiconsult_V001.Classes
{
    public class Geo_Layer
    {
        public int id;
        public string name;
        public Brep brep;
        public List<Geo_Node> nodes;


        public Geo_Layer()
        { 
            //empty constructors
        }
    }
}
