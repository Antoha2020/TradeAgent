using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class Form1 : Form
    {
        private DBHandler db;
        GMapOverlay markers = new GMapOverlay("markers");//все маркеры, которые есть на карте
        
        public Form1()
        {
            InitializeComponent();
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;

            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.SetPositionByKeywords("Paris, France");
            gMapControl1.ShowCenter = false;

            GMapOverlay markers = new GMapOverlay("markers");
            GMapMarker marker = new GMarkerGoogle(
                new PointLatLng(48.8617774, 2.349272),
                GMarkerGoogleType.blue_pushpin);
            markers.Markers.Add(marker);
            gMapControl1.Overlays.Add(markers);
            db = new DBHandler();
            db.DBConnection();
            
        }        

        private void googleMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void спутниковаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GoogleSatelliteMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void другаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.YandexMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // textBox1.Text = db.getDataTEST();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Fact fact = new Fact(gMapControl1);
            fact.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Plan plan = new Plan();
            plan.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Reports reports = new Reports();
            reports.Show();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(48.489, 34.993);
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;
            gMapControl1.DragButton = MouseButtons.Left;
        }

        
        private void gMapControl1_OnMapZoomChanged()
        {
            trackBar1.Value = (int)gMapControl1.Zoom;
        }

        RouterDb routerDb;
        private void toolStripButton6_Click(object sender, EventArgs e)// тестовая кнопка, убрать
        {

             markers.Clear();
             gMapControl1.Overlays.Clear();
             List<PointLatLng> list = new List<PointLatLng>();
             List<int> numbers = new List<int>();
             Random rnd = new Random();
             for (int i = 0; i < 2; i++)
             {
                 numbers.Add(rnd.Next(1, 20000));
             }
             list = db.getPointsTEST(numbers);
             foreach (PointLatLng pt in list)
                 markers.Markers.Add(new GMarkerGoogle(pt, GMarkerGoogleType.green_small));

             gMapControl1.Overlays.Add(markers);
             gMapControl1.Zoom++;
             gMapControl1.Zoom--;
             gMapControl1.Refresh();

            //---------------------------------------------------------------------------------
            /* StreamReader reader = new StreamReader("004075.gps");
             string s = "";
             double latRoute = 0, lngRoute = 0;
             long tm = 0;
             List<double> lt = new List<double>();
             List<double> ln = new List<double>();
             List<long> time = new List<long>(); 
             while (true)
             {
                 s = reader.ReadLine();
                 if (s == null || s == "")
                     break;

                string [] StrArray1 = s.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                latRoute = Convert.ToDouble(StrArray1[1]) / 100 - (int)Convert.ToDouble(StrArray1[1]) / 100;
                lngRoute = Convert.ToDouble(StrArray1[2]) / 100 - (int)Convert.ToDouble(StrArray1[2]) / 100;

                 latRoute = (int)Convert.ToDouble(StrArray1[1]) / 100 + latRoute * 100 / 60;
                 lngRoute = (int)Convert.ToDouble(StrArray1[2]) / 100 + lngRoute * 100 / 60;

                 lt.Add(latRoute);
                 ln.Add(lngRoute);

                 DateTimeOffset dt = DateTimeOffset.ParseExact(StrArray1[0], "dd.MM.yyyy HH:mm.ss", null); //Convert.ToDateTime(StrArray1[0]);


                 tm = dt.ToUnixTimeMilliseconds();
                 time.Add(tm);
             }

             db.insertFactTEST(lt, ln, time);*/
            //--------------------------------------------------------------------------------------

            /* GMapRoute r = new GMapRoute(db.getDataTEST(), "Route");
             r.IsVisible = true;
             r.Stroke = new Pen(Color.Green, 2);
             // r.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
             markers.Routes.Add(r);

             gMapControl1.Overlays.Add(markers);
             gMapControl1.Refresh();*/

           /* gMapControl1.Cursor = Cursors.WaitCursor;
            routerDb = new RouterDb();
            using (var stream = File.OpenRead("Dnepr.pbf"))
            {
                routerDb.LoadOsmData(stream, Vehicle.Car);
            }
            routerDb.AddContracted(Vehicle.Car.Fastest());

            // comboBox1.Enabled = false;
            GetDistRouteOSM(list[0], list[1], true);
            gMapControl1.Cursor = Cursors.Default;*/
        }

        private double GetDistRouteOSM(Point Pa, Point Pb, bool Table)//расчет расстояния из OSM файла
        {
            List<PointLatLng> list = new List<PointLatLng>();
            List<GMapRoute> ListRoute = new List<GMapRoute>();
            double Dist = 0;
            try
            {
                Router router = new Router(routerDb);
                var route = router.Calculate(Vehicle.Car.Shortest(), new Coordinate((float)Pa.X, (float)Pa.Y),
                     new Coordinate((float)(Pb.X), (float)(Pb.Y)));
                var routeGeoJson = route.ToGeoJson();

                JObject CoordinateSearch = JObject.Parse(routeGeoJson.ToString());
                IList<JToken> results = CoordinateSearch["features"].Children().ToList();
                IList<SearchResult> searchResults = new List<SearchResult>();

                SearchResult searchResult;
                foreach (JToken result in results)
                {
                    if (!result.ToString().Contains("\"type\": \"Point\""))
                    {
                        searchResult = JsonConvert.DeserializeObject<SearchResult>(result.ToString());
                        searchResults.Add(searchResult);
                        for (int d = 0; d < searchResult.geometry.coordinates.Length / 2; d++)
                            list.Add(new GMap.NET.PointLatLng(searchResult.geometry.coordinates[d, 1], searchResult.geometry.coordinates[d, 0]));
                        Dist = Math.Round(Convert.ToDouble(searchResult.properties.distance), 3);
                    }

                    /*GMapRoute r = new GMapRoute(list, "Route");
                    r.IsVisible = true;
                    r.Stroke.Color = Color.Red;//.DarkGreen;
                    ListRoute.Add(r);*/

                }
                GMapRoute r = new GMapRoute(list, "Route");
                r.IsVisible = true;
                r.Stroke = new Pen(Color.Blue, 3);
                ListRoute.Add(r);

               // ErrorOSM = false;
                /*if (Pb.CodeTradePoint != "Start")// || !Table)
                {
                    Pb.X = Pb.X + DeltaXOSM;
                    Pb.Y = Pb.Y + DeltaYOSM;
                }*/
            }
            catch
            {
                //ErrorOSM = true;
                return 0;
                //ListNoNodes.Add(Pb);
                /*Dist = Math.Round(Math.Sqrt(Math.Pow((Pa.X - Pb.X) * Constant.ParallelDist, 2) + Math.Pow((Pa.Y - Pb.Y) * Constant.MeridianDist, 2)), 3);
                list.Add(new PointLatLng(Pa.X, Pa.Y));
                list.Add(new PointLatLng(Pb.X, Pb.Y));
                GMapRoute r = new GMap.NET.WindowsForms.GMapRoute(list, "Route");
                r.IsVisible = true;
                r.Stroke.Color = Color.DarkGreen;*/
                //ListRoute.Add(r);

                //list.Add(new GMap.NET.PointLatLng(searchResult.geometry.coordinates[d, 1], searchResult.geometry.coordinates[d, 0]));
            }

           // Pb.RouteFromPrev = ListRoute;
           // if (Dist < 10)
             //   Dist = 0;
            return Math.Round(Dist / 1000, 3);
        }

       /* private void ReDrawPoints()
        {
            gMapControl1.Overlays.Clear();
            for (int r = markersOverlay.Markers.Count - 1; r >= 0; r--)
                markersOverlay.Markers.RemoveAt(r);

            for (int i = 0; i < Reg.TradePoints.Count; i++)
            {
                GMarkerGoogle marker = null;
                if (Reg.TradePoints[i].CodeTradePoint == "Start")
                    marker = new GMarkerGoogle(new PointLatLng(Reg.TradePoints[i].X, Reg.TradePoints[i].Y), GMarkerGoogleType.pink_dot);
                else
                    marker = new GMarkerGoogle(new PointLatLng(Reg.TradePoints[i].X, Reg.TradePoints[i].Y), col[Reg.TradePoints[i].Color]);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTipText = Reg.TradePoints[i].Number.ToString() + "_" + Reg.TradePoints[i].Adress;
                markersOverlay.Markers.Add(marker);
            }
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Refresh();
        }*/
    }
}
