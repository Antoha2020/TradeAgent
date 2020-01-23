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
        public Dictionary<long, PointLatLng> listPoints = new Dictionary<long, PointLatLng>();
        public List<TradePoint> tradePoints = new List<TradePoint>();
        public Route(Dictionary<long, PointLatLng> listPoints)
        {
            this.listPoints = listPoints;//ключ-время (UTC таймстемп) значение - узловая точка

        }
    }
}
