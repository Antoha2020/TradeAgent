using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class Constants
    {
        public static string HOST = "35.198.91.129";
        public static int PORT = 3306;
        public static string DATABASE = "logistics";
        public static string USERNAME = "userdb";
        public static string PASSWORD = "Kai2019%";
        public static double EARTH_RADIUS = 6372795;//радиус земного шара, м

        public static double getDistance(double lt1, double ln1, double lt2, double ln2)//вычисляет расстояние между двумя точками
        {
            // перевести координаты в радианы
            double lt1Rad = lt1 * Math.PI / 180;
            double ln1Rad = ln1 * Math.PI / 180;
            double lt2Rad = lt2 * Math.PI / 180;
            double ln2Rad = ln2 * Math.PI / 180;

            // косинусы и синусы широт и разницы долгот
            double coslt1Rad = Math.Cos(lt1Rad);
            double coslt2Rad = Math.Cos(lt2Rad);
            double sinlt1Rad = Math.Sin(lt1Rad);
            double sinlt2Rad = Math.Sin(lt2Rad);

            double delta = ln2Rad - ln1Rad;
            double cos_delta = Math.Cos(delta);
            double sin_delta = Math.Sin(delta);

            // вычисления длины большого круга
            double y = Math.Sqrt(Math.Pow(coslt2Rad * sin_delta, 2) + Math.Pow(coslt1Rad * sinlt2Rad - sinlt1Rad * coslt2Rad * cos_delta, 2));
            double x = sinlt1Rad * sinlt2Rad + coslt1Rad * coslt2Rad * cos_delta;

            double ad = Math.Atan2(y, x);
            double dist = ad * EARTH_RADIUS;

            //return Math.Round(dist/1000,3);
            return dist;
        }
    }
}
