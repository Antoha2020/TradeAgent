using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class TradePoint
    {
        private double latitude;//широта расположения торговой точки
        private double longitude;//долгота расположения торговой точки
        private string address { get; set; }//адрес расположения торговой точки
        private string name { get; set; } //название торговой точки
        private string code;//код торговой точки
        private string owner;//владелец торговой точки


        public double Latitude
        {
            get { return latitude; }
            set { this.latitude = value >= 0 ? value : 0; }
        }

        public double Longitude
        {
            get { return longitude; }
            set { this.longitude = value >= 0 ? value : 0; }
        }

        public string Address
        {
            get { return address; }
            set { this.address = value; }
        }

        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        public string Code
        {
            get { return code; }
            set { this.code = value; }
        }

        public string Owner
        {
            get { return owner; }
            set { this.owner = value.Contains("@") ? value : "0"; }//проверка на наличие в коде ТТ знака @  возможно убрать
        }
    }
}
