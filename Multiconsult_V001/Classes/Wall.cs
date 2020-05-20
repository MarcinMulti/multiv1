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
        public Brep surface;
        public Point3d[] nodes;
        public Curve boundary;
        public Curve[] holes;
        public Curve bottomAxis;
        public Curve topAxis;
        public Curve[] bottomCurves;
        public Curve[] topCurves;
        public Plane planeHorizontal;
        public Plane planeBottom;
        public Plane planeTop;
        public Plane plane;
        public Brep brep;
        public Line vectorHeight;
        public double height;
        public Line[] constructionLines;

        public string name;
        public int id;
        public Wall_Section section;
        public Material material;

        //constructors
        public Wall()
        {
            //empty constructor
        }
        public Wall(string _name)
        {
            name = _name;
        }
        public Wall(Brep _surface)
        {
            surface = _surface;
        }

        public Wall(int _id, Brep _surface)
        {
            id = _id;
            surface = _surface;
        }
    }
}
