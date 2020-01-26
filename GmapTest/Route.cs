using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    [Serializable]
    class Route
    {
        string codeGPS;//imei gps-устройства
        string name;//наименование маршрута
        double latBeg;//широта базовой точки начала маршрута
        double lonBeg;//долгота базовой точки начала маршрута
        string team;//название торговой команды, которой принадлежит маршрут
        string region;//регион, в котором работает данная торговая команда

        public string CodeGPS { get { return codeGPS; } }
        public string Name { get { return name; } }
        public double LatBeg { get { return latBeg; } }
        public double LonBeg { get { return lonBeg; } }
        public string Team { get { return team; } }
        public string Region { get { return region; } }

        public Route(string codeGPS, string name, string latBeg, string lonBeg, string team, string region)
        {
            this.codeGPS = codeGPS;
            this.name = name;
            this.team = team;
            this.region = region;
            if (latBeg != null && latBeg != "")
                this.latBeg = Convert.ToDouble(latBeg);
            if (lonBeg != null && lonBeg != "")
                this.lonBeg = Convert.ToDouble(lonBeg);
        }
    }
}
