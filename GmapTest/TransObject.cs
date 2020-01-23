using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class TransObject
    {
        private string garageNumber;
        private string model;
        private string imei;

        public TransObject(string garageNumber, string model, string imei)
        {
            this.garageNumber = garageNumber;
            this.model = model;
            this.imei = imei;
        }
        public string GarageNumber
        {
            get { return garageNumber; }
            set { this.garageNumber = value; }
        }

        public string Model
        {
            get { return model; }
            set { this.model = value; }
        }

        public string Imei
        {
            get { return imei; }
            set { this.imei = value; }
        }
    }
}
