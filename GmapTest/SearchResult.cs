using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{

    public class SearchResult
    {
        public string name { get; set; }
        public Prop properties;
        public Geometry geometry;
    }
    public class Geometry
    {
        public string type { get; set; }
        double[,] Coordinates;

        public double[,] coordinates
        {
            set
            {
                if (type != "Point")
                {
                    Coordinates = value;
                }
            }
            get { return Coordinates; }
        }
    }
    public class Prop
    {
        public double distance { get; set; }
    }
}
