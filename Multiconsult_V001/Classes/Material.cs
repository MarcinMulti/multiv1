using GH_IO.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Material
    {
        public string name; //multiconsult name of the material
        public int id;
        public int type;
        public string category; //the main material name: concrete, steel, timber ...
        public string subcategory; // the specific type of material C20/25, S355, ...

        //FEM design properties
        public int FEMDesignType;   //the integer from material list, from FEMDesign 
        public string FEMDesignName;

        //Revit properties
        public string RevitMaterialName;
        

        public Material()
        {
            //empty constructor
        }
        public Material(string _name)
        {
            name = _name;
        }
    }
}
