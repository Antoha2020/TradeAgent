using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class Constants
    {
        public const string HOST = "35.198.91.129";
        public const int PORT = 3306;
        public const string DATABASE = "logistics";
        public const string USERNAME = "userdb";
        public const string PASSWORD = "Kai2019%";
        public const double EARTH_RADIUS = 6372795;//радиус земного шара, м
        public const double STOP_TIME_IN_POINT = 60;//время в секундах, которые считаются остановкой
        public const int CRITICAL_DISTANCE = 2500;//максимальное расстояние между строчками факта, после которого остановка не считается...
        public const double RADIUS_SMOOTH = 50;//радиус окружности, в границах которой сглаживаются маршруты
        public const double DegInRad = Math.PI / 180;// 0.0174533;//1 градус в радианах
        public const int ParallelDist = 111000;//расстояние между параллелями (48)
        public const int MeridianDist = 74000;//примерное расстояние между меридианами для Днепропетровской обл (35)
        public static double RADIUS = 160; //величина радиуса, в котором определяются точки посещения
        public static int NUM_CORNERS = 20;//количество углов в многоугольнике, который принимается за окружность 

        public const string Path_fact = "./Fact routes";
        public const string Path_db = "./DB";
        public const string Path_plan = "./Plan routes/";//путь, по которому находятся плановые маршруты
        //public const string Path_plan = "./Plan routes/";//путь, по которому находятся плановые маршруты

        public static bool firstDraw = false;
        public static int EmptyCell = 1000000;//признак пустой ячейки
        public static int FirstMinCell = 10000;//начальное значение минимального элемента
        public static double SpeedCarHW = 45;//скорость на машине по трассе
        public static double SpeedCar = 32;//скорость на машине
        public static double SpeedFoot = 3;//скорость пешком
        public static int MaxVisitTP = 1;
        public static int EndRt = 1000000;
        public static string NameProg = "Расчет маршрутов торговых представителей";
        public static int MaxIterations = 50000;//максимальное количество попыток изменения привязки точки, 
                                                //после превышения просит перепривязать точку вручную
        public static double MinDeltaXOSM = 0.00001;
        public static double MinDeltaYOSM = 0.00001;
        public static double WorkTime = 3.5;//продолжительность рабочего времени

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

        public static int GetSeconds(DateTime dt)
        {
            return dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
        }
    }
}
