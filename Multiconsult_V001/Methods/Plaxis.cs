using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Multiconsult_V001.Methods
{
    class Plaxis
    {

        //method to zero the z coordinates in list od points
        public static List<Point3d> ProjectPointsOnXY(List<Point3d> pts)
        {            
            List<Point3d> fPts = new List<Point3d>();

            foreach (var p in pts)
            {
                fPts.Add(new Point3d(p.X,p.Y,0));
            }

            return fPts;
        }
    }
}
