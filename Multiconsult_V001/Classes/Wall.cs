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

        //methods
        public void refreshConstructionLineFromAxis()
        {
            //line
            var botLine = new Line(bottomAxis.PointAtStart, bottomAxis.PointAtEnd);
            var topLine = new Line(topAxis.PointAtStart, topAxis.PointAtEnd);

            var v1Line = new Line(botLine.From, topLine.From);
            var v2Line = new Line(botLine.To, topLine.To);

            var cenPoint = new Line(botLine.PointAt(0.5), topLine.PointAt(0.5)).PointAt(0.5);

            //vector from old to new point
            double w = section.width;

            //construction lines
            Line cl0 = new Line(v1Line.PointAt(0.5), v2Line.PointAt(0.5));
            Line cl1 = constructionLines[1];
            Line cl2 = new Line(botLine.PointAt(0.5), topLine.PointAt(0.5));

            var lonvec1 = Point3d.Subtract(cl1.To,cl1.From);
            lonvec1.X = lonvec1.X / 2;
            lonvec1.Y = lonvec1.Y / 2;
            lonvec1.Z = lonvec1.Z / 2;

            var lonvec2 = Point3d.Subtract(cl1.To, cl1.From);
            lonvec2.X = -lonvec2.X / 2;
            lonvec2.Y = -lonvec2.Y / 2;
            lonvec2.Z = -lonvec2.Z / 2;

            cl1 = new Line(Point3d.Add(cenPoint, lonvec1), Point3d.Add(cenPoint, lonvec2));

            plane = new Plane(cenPoint,lonvec1);
            constructionLines = new Line[3] { cl0,cl1,cl2 };

        }

        public void makeMainSurfaceFromAxis()
        {
            //line
            Curve rail = new Line(bottomAxis.PointAtStart, topAxis.PointAtStart).ToNurbsCurve();

            List<Curve> sections = new List<Curve>();
            sections.Add(bottomAxis);
            sections.Add(topAxis);

            //vector from old to new point
            Brep mastersurface = Brep.CreateFromSweep(rail, sections, true, 0.000001)[0];

            surface = mastersurface;
        }
    }
}
