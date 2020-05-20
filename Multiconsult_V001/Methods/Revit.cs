using Multiconsult_V001.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Methods
{
    class Revit
    {
        public static Material getRevitMaterialFromString(string type)
        {
            Material m = new Material();

            //get materials from revit string
            string[] RevitMats = type.Split(':');
            m.RevitMaterialName = RevitMats[1];
            string[] matName = type.Split('-');
            m.name = matName[1].Trim();

            return m;
        }
    }
}
