using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace GmapTest
{
    public partial class Fact : Form
    {
        private DBHandler db;
        public GMap.NET.WindowsForms.GMapControl gmapControl1 = new GMapControl();
        int counter = 0;
        public Fact(GMap.NET.WindowsForms.GMapControl gmapControl1)
        {
            InitializeComponent();
            db = new DBHandler();
            db.DBConnection();

            this.gmapControl1 = gmapControl1;
            counter++;
        }

        private void Fact_Load(object sender, EventArgs e)
        {
            List<TransObject> tObj = new List<TransObject>();
            tObj = db.getTransObjects();
            foreach (TransObject tobj in tObj)
            {
                dataGridView1.Rows.Add(false, tobj.GarageNumber, tobj.Model, tobj.Imei);
            }
        }

        List<PointLatLng> routePoints=new List<PointLatLng>();//в дальнейшем убрать
        Dictionary<long, PointLatLng> routePointsDic = new Dictionary<long, PointLatLng>();
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString().ToLower().Equals("true"))
                {
                    GMapOverlay markers = new GMapOverlay("markers");
                    string table = "route" + dataGridView1.Rows[i].Cells[3].Value.ToString() + "_" + dateTimePicker1.Value.Day.ToString() +
                        dateTimePicker1.Value.Month.ToString() + dateTimePicker1.Value.Year.ToString();
                    table = "route_phone";//убрать
                    routePoints = db.getDataListTEST(table);
                    /*foreach(KeyValuePair<long,PointLatLng> dic in routePointsDic)
                    {
                        routePoints.Add(dic.Value);
                    }*/

                    GMapRoute r = new GMapRoute(routePoints, "Route");
                    r.IsVisible = true;
                    r.Stroke = new Pen(dataGridView1.Rows[i].Cells[4].Style.BackColor, 3);
                    markers.Routes.Add(r);
                    gmapControl1.Position = routePoints[routePoints.Count - 1];
                    gmapControl1.Overlays.Add(markers);
                    gmapControl1.Zoom = 10;
                    gmapControl1.Refresh();
                }
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            gmapControl1.Overlays.Clear();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = false;
            }
            gmapControl1.Zoom = 10;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex==4)
            {               
                if(colorDialog1.ShowDialog() == DialogResult.OK)
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = colorDialog1.Color;
                dataGridView1.CurrentCell = null;
            }
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Route rt = new Route(db.getDataTEST("route1111_18122019"));
            //получить сериализованную строку байтов
            string s = "";
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, rt);
            byte[] bytes = ms.ToArray();
            s = BitConverter.ToString(bytes);
            s = s.Replace("-", "");
            db.insertSerializeTEST("1111","18.12.2019", s);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = db.getSerialTEST();
            byte[] bytes = Enumerable.Range(0, str.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
                                    .ToArray();

            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            Route rt = new Route(new Dictionary<long, PointLatLng>());
            rt = (Route)formatter.Deserialize(stream);

            List<PointLatLng> routePoints1 = new List<PointLatLng>();
            foreach (KeyValuePair<long, PointLatLng> dic in rt.listPoints)
            {
                routePoints1.Add(dic.Value);
            }

            GMapOverlay markers = new GMapOverlay("markers");
            GMapRoute r = new GMapRoute(routePoints1, "Route");
            r.IsVisible = true;
            r.Stroke = new Pen(Color.Red, 3);
            markers.Routes.Add(r);
            gmapControl1.Position = routePoints1[routePoints1.Count - 1];
            gmapControl1.Overlays.Add(markers);
            gmapControl1.Zoom = 12;
            gmapControl1.Refresh();
        }
    }
}
