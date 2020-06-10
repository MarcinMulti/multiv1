using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiconsult_V001.Classes
{
    class Geo_Soil
    {
        public int id;
        public string name;
        public List<Geo_Borehole> boreholes;
        public List<Geo_Surface> geosurfaces;
        public Geo_Terrain terrain;
        public List<Geo_Layer> layers;

        public Geo_Soil()
        { 
        }


    }
}
