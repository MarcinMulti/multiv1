﻿using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Column
    {
        //properties
        public Line line;
        public Point3d pt_st;
        public Point3d pt_end;
        public string name;
        public int id;
        public string section;
        public string material;

        //constructors
        public Column(Line _line)
        {
            line = _line;
            pt_st = line.From;
            pt_end = line.To;
        }

        public Column(int _id, Line _line)
        {
            id = _id;
            line = _line;
            pt_st = line.From;
            pt_end = line.To;
        }

    }
}