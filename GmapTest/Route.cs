using GMap.NET;
using System;
using System.Collections.Generic;
using System.IO;
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
            Logger.Log.Info("Создан объект Route");
        }

        public static List<PointLatLng> getPlanRoute(string FilePath)
        {
            List<PointLatLng> RoutePlan = new List<PointLatLng>();
            string[] StrPlan;
            if (!File.Exists(FilePath))
                return null;

            StreamReader reader = new StreamReader(FilePath, Encoding.Default);
            string s = "";
            while (true)
            {
                s = reader.ReadLine();
                if (s == null || s == "")
                    break;

                StrPlan = s.Split(new Char[] { '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (StrPlan[0] == "**")
                    continue;
                if (StrPlan[0] != "p")
                    RoutePlan.Add(new PointLatLng(Convert.ToDouble(StrPlan[0]), Convert.ToDouble(StrPlan[1])));
            }
            return RoutePlan;
        }
    }
}
