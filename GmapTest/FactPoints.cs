using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    public class FactPoints
    {
        public double X;
        public double Y;
        public double Time;//время в точке, с
        public DateTime TimeFrom, TimeTo;//время прибытия и отправления из точки
        public List<Point> ListPointsInside = new List<Point>();//список плановых точек внутри круга
        public List<double> SectorX = new List<double>();
        public List<double> SectorY = new List<double>();
        public double DistFromBegin = 0;
        public FactPoints(double X, double Y, DateTime TimeFrom, DateTime TimeTo, double DistFromFirst)
        {
            this.X = X;
            this.Y = Y;
            this.TimeFrom = TimeFrom;
            this.TimeTo = TimeTo;
            DistFromBegin = DistFromFirst;
            Time = Constants.GetSeconds(TimeTo) - Constants.GetSeconds(TimeFrom);
            FindCircle();
        }

        public int InsidePolygonFP(List<double> xp, List<double> yp, List<Point> TradePoints)
        {
            List<Point> Lpts = new List<Point>();
            foreach (Point TrPoint in TradePoints)
            {
                int intersections_num = 0;
                int prev = xp.Count - 1;
                bool prev_under = yp[prev] < TrPoint.Y;

                for (int i = 0; i < xp.Count; ++i)
                {
                    bool cur_under = yp[i] < TrPoint.Y;

                    double ax = xp[prev] - TrPoint.X;
                    double ay = yp[prev] - TrPoint.Y;

                    double bx = xp[i] - TrPoint.X;
                    double by = yp[i] - TrPoint.Y;

                    double t = (ax * (by - ay) - ay * (bx - ax));
                    if (cur_under && !prev_under)
                    {
                        if (t > 0)
                            intersections_num += 1;
                    }
                    if (!cur_under && prev_under)
                    {
                        if (t < 0)
                            intersections_num += 1;
                    }

                    prev = i;
                    prev_under = cur_under;
                }

                if (intersections_num % 2 == 1)
                {
                    if (!TrPoint.IsVisited && TrPoint.Adress != "Start")
                    {
                        Lpts.Add(TrPoint);
                        ListPointsInside.Add(TrPoint);
                        TrPoint.IsVisited = true;
                        TrPoint.FactDistFromBeg = DistFromBegin;
                    }
                }
            }

            if (Lpts.Count != 0)
            {
                int TimeInPt = Convert.ToInt32(Time / Lpts.Count);//общее время простоя в точке остановки
                DateTime dtm = TimeFrom;
                foreach (Point p in Lpts)
                {
                    p.FactTimeArrive = dtm;
                    p.FactTimeDepart = p.FactTimeArrive.AddSeconds(TimeInPt);
                    dtm = p.FactTimeDepart;
                }
            }
            return ListPointsInside.Count;
        }

        private void FindCircle()
        {
            double MainAngle = 0;
            double OtherAngle = 0;
            double MaxDist = Convert.ToDouble(Constants.RADIUS) / 1000;
            double Basis = 0;

            double LastPointX = X + MaxDist / (Constants.ParallelDist / 1000);
            double LastPointY = Y;

            for (int i = 0; i < Constants.NUM_CORNERS; i++)
            {
                MainAngle += Constants.DegInRad * 360 / Constants.NUM_CORNERS;
                OtherAngle = (Math.PI - MainAngle) / 2;
                Basis = 2 * (MaxDist * Math.Sin(MainAngle / 2));

                LastPointX = (X + MaxDist / (Constants.ParallelDist / 1000)) - (Math.Cos(OtherAngle) * Basis) / (Constants.ParallelDist / 1000);
                LastPointY = Y + (Math.Sin(OtherAngle) * Basis) / (Constants.MeridianDist / 1000);
                SectorX.Add(LastPointX);
                SectorY.Add(LastPointY);

            }

        }
    }
}
