using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GmapTest
{
    public class Point
    {
        public List<double> SectorX = new List<double>();//для КГС
        public List<double> SectorY = new List<double>();//для КГС

        public double FactDistFromBeg;//фактическое расстояние от включения gps до данной точки
        public DateTime FactTimeArrive = DateTime.MaxValue;//фактическое время прибытия в точку
        public DateTime FactTimeDepart = DateTime.MinValue;//фактическое время отправления из точки
        public double FactTimeInPoint = 0;//фактическое время нахождения в точке в минутах
        public bool IsVisited;//false - точка не посещалась, true - посещение произошло
        public int Number;//порядковый номер точки
        public string CodeTradePoint;//код торговой точки из 1С        
       
        public string Adress;//адрес ТТ
        public double X;//координата х ТТ
        public double Y;//координата у ТТ
        public double X_real;//координата х ТТ
        public double Y_real;//координата у ТТ
        
        public double Time;//время обслуживания ТТ
        
        public double TimeDepart;//время движения от первой точки до отправления с данной
        
        public double TimeArrive;//время движения от первой точки до прибытия на данную
        
        public double DistanceFromFirst;//расстояние от первой точки
        
        public int NumVisit;//количество посещений в неделю
        
        public bool InPoly;//флаг нахождения точки в выделенном полигоне
        
        public string WeekDay;//день недели посещения точки Mon,Tues,Wed,Thur,Fri,MonThur,TuesFri
        
        public bool InWork;
        
        public bool RiverSide;//false - ТТ на левом берегу Днепра, true - на правом 
        public int VicinityPoints;//количество точек, находящихся в окрестности
        public int Sector;//номер сектора, в который попадает точка
        public int CarZone;//номер машинозоны в секторе, начиная с крайней точки
        
        public int Color;//цвет маркера торговой точки
        
        public bool MaxDistZone;//true - Максимально удаленная точка в маш-зоне от центра окружности
        
        public bool MinDistZone;//true - Минимально удаленная точка в маш-зоне от центра окружности
        
        public double DistZone;//расстояние по прямой от центра окружности до точки
        
        public bool N;//если true, то точка уже включена в маршрут по наименьшим расстояниям
        
        public double DistPrevTP;//расстояние от предыдущей ТТ
       
        public bool InRoute;//true-точка уже в маршруте
        
        public List<GMapRoute> RouteFromPrev = new List<GMapRoute>();//маршрут от предыдущей точки
        
        public bool LastGrey;//точка раньше была серой
        public string Brand;//бренд, которому принадлежит точка
        public string RouteName;//название маршрута, которому принадлежит точка

        //поля для аналитики
        /*public string BrandAnalitics;//бренд, которому принадлежит точка в файле аналитики
        public double WeightGoods;//средняя масса отгрузки за выбранный период
        public double Turnover;//товарооборот, деньги, полученные за товар, грн
        public double Size;//средний объем груза в точке
        public double Probability;//вероятность срабатывания точки
        public int MaxWork;//макисимально возможное количество срабатываний точки за данный период
        public double RealWork;//реальное количество срабатываний точки в данный период*/

        public Point()
        {
        }

        public Point(int Number, string CodeTradePoint, string Adress, double X, double Y, double Time)
        {
            this.CodeTradePoint = CodeTradePoint;
            this.Number = Number;
            this.Adress = Adress;
            this.X = X;
            this.Y = Y;
            X_real = X;
            Y_real = Y;
            this.Time = Math.Round(Time / 60, 2);
            TimeDepart = Math.Round(Time / 60, 2);
            DistanceFromFirst = 0;
            this.NumVisit = 1;//NumVisit;
            InPoly = false;
            WeekDay = "NO";
            InWork = false;
            RiverSide = false;
            Color = 0;
            MaxDistZone = false;
            MinDistZone = false;
            DistZone = 0;
            N = false;
            DistPrevTP = 0;
            CarZone = 0;
            InRoute = false;
            LastGrey = false;
            Brand = "";
            //BrandAnalitics = "";
           /* WeightGoods = 0;
            Turnover = 0;
            Size = 0;
            Probability = 0;
            MaxWork = 0;
            RealWork = 0;
            IsVisited = false;*/
        }

        public void RemoveListRoute()
        {
            for (int i = RouteFromPrev.Count - 1; i >= 0; i--)
                RouteFromPrev.RemoveAt(i);
        }

        public void AddParam(string Br, string Rt, string WD, int cl)
        {
            this.Brand = Br;
            this.RouteName = Rt;
            this.WeekDay = WD;
            this.Color = cl;
        }
        public void SetDistTime(double DistPrevTP, double DistanceFromFirst, double TimeArrive, double TimeDepart)
        {
            this.DistPrevTP = DistPrevTP;
            this.DistanceFromFirst = DistanceFromFirst;
            this.TimeArrive = TimeArrive;
            this.TimeDepart = TimeDepart;
        }
    }
}
