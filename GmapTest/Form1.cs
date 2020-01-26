using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class Form1 : Form
    {
        List<Route> listRoutes = new List<Route>();
        string DirName = "";
        String dbFileNameAutorization = Constants.Path_db + "/Authorization_db.db";
        private DBHandler db;
        GMapOverlay markers = new GMapOverlay("markers");//все маркеры, которые есть на карте

        GMapOverlay markersOverlayStartFin = new GMapOverlay("marker");
        private bool StartFinish = false;
        double FirstLat = 0, FirstLng = 0;
        private List<PointLatLng> TwoPointDist = new List<PointLatLng>();
        double SumDist = 0;
        int CountPts = 1;
        public Dictionary<string, List<Point>> PointsInRoute = new Dictionary<string, List<Point>>();
        public Dictionary<string, List<FactPoints>> LFP = new Dictionary<string, List<FactPoints>>();

        public Form1()
        {
            InitializeComponent();
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            Logger.Log.Info("Start main form");
            listRoutes = DBHandler.GetListRoutes();//получение всех маршрутов, которые есть в системе
            SetComboBox();
        }        

        private void SetComboBox()
        {
            foreach(Route rt in listRoutes)
            {
                if (!comboBox1.Items.Contains(rt.Team))
                    comboBox1.Items.Add(rt.Team);
                if (!comboBox2.Items.Contains(rt.Region))
                    comboBox2.Items.Add(rt.Region);                
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;
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
            //Fact fact = new Fact(gMapControl1);
            //fact.Show();
            toolStripButton2.Checked = false;
            if (toolStripButton1.Checked)
            {
                panel1.Visible = true;
                panel2.Visible = false;

            }
            else
                panel1.Visible = false;

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            if (toolStripButton2.Checked)
            {
                panel1.Visible = false;
                panel2.Visible = true;
            }
            else
                panel2.Visible = false;
            //Plan plan = new Plan();
            //plan.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //Reports reports = new Reports();
            //reports.Show();
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
            //
             /*markers.Clear();
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
             */
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

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {


            /*List<double> xp = new List<double>();
            List<double> yp = new List<double>();
            if (e.Button == MouseButtons.Right)
            {
                textBox2.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat.ToString();
                textBox4.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng.ToString();
            }*/
            if (toolStripButton4.Checked)
            {
                GMarkerGoogle marker;
                double Lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                double Lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;
                if (e.Button == MouseButtons.Right)
                {

                    if (!StartFinish)
                    {
                        FirstLat = Lat;
                        FirstLng = Lng;
                        marker = new GMarkerGoogle(new PointLatLng(FirstLat, FirstLng), GMarkerGoogleType.blue_pushpin);

                        marker.ToolTip = new GMapRoundedToolTip(marker);
                        marker.ToolTipText = "Точка " + CountPts.ToString() + ":\n" + Math.Round(FirstLat, 5).ToString() +
                            "  " + Math.Round(FirstLng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";
                        markersOverlayStartFin.Markers.Add(marker);
                        TwoPointDist.Add(new PointLatLng(FirstLat, FirstLng));
                        StartFinish = true;
                    }
                    else
                    {
                        marker = new GMarkerGoogle(new PointLatLng(Lat, Lng), GMarkerGoogleType.pink_pushpin);
                        marker.ToolTip = new GMapRoundedToolTip(marker);

                        SumDist += Math.Round(Constants.getDistance(FirstLat, FirstLng, Lat, Lng) / 1000, 3);
                        marker.ToolTipText = "Точка " + (++CountPts).ToString() + ":\n" + Math.Round(Lat, 5).ToString() +
                            "  " + Math.Round(Lng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";

                        markersOverlayStartFin.Markers.Add(marker);

                        TwoPointDist.Add(new PointLatLng(Lat, Lng));
                        GMapRoute r = new GMapRoute(TwoPointDist, "Route");
                        r.IsVisible = true;
                        r.Stroke = new Pen(Color.Blue, 2);
                        markersOverlayStartFin.Routes.Add(r);

                        FirstLat = Lat;
                        FirstLng = Lng;
                    }

                }


                if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Shift) == Keys.Shift && StartFinish)
                {
                    marker = new GMarkerGoogle(new PointLatLng(Lat, Lng), GMarkerGoogleType.blue_pushpin);
                    marker.ToolTip = new GMapRoundedToolTip(marker);

                    SumDist += Math.Round(Constants.getDistance(FirstLat, FirstLng, Lat, Lng) / 1000, 3);
                    marker.ToolTipText = "Точка " + (++CountPts).ToString() + ":\n" + Math.Round(Lat, 5).ToString() +
                        "  " + Math.Round(Lng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";

                    markersOverlayStartFin.Markers.Add(marker);

                    TwoPointDist.Add(new PointLatLng(Lat, Lng));
                    GMapRoute r = new GMapRoute(TwoPointDist, "Route");
                    r.IsVisible = true;
                    r.Stroke = new Pen(Color.Blue, 2);
                    markersOverlayStartFin.Routes.Add(r);
                    SumDist = 0;

                    StartFinish = false;
                    TwoPointDist.Clear();
                    CountPts = 1;
                }

                gMapControl1.Overlays.Add(markersOverlayStartFin);
                gMapControl1.Zoom += 0.1;
                gMapControl1.Zoom -= 0.1;
            }
            /* if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
             {
                 textBox1.Text = "Записать в файл";
                 textBox1.Enabled = false;

                 if (ChangeSector.Count > 0)// в дальнейшем возможно убрать
                 {
                     for (int i = polyOverlayChange.Polygons.Count - 1; i >= 0; i--)
                         polyOverlayChange.Polygons.RemoveAt(i);
                     for (int i = polyOverlayBordersPolyChange.Polygons.Count - 1; i >= 0; i--)
                         polyOverlayBordersPolyChange.Polygons.RemoveAt(i);
                     for (int i = ChangeSector.Count - 1; i >= 0; i--)
                         ChangeSector.RemoveAt(i);
                 }
                 pointsHalfWeek.Add(new PointLatLng(gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng));
                 if (pointsHalfWeek.Count > 1)
                 {
                     List<PointLatLng> TwoPoints = new List<PointLatLng>();
                     TwoPoints.Add(pointsHalfWeek[pointsHalfWeek.Count - 2]);
                     TwoPoints.Add(pointsHalfWeek[pointsHalfWeek.Count - 1]);

                     GMapPolygon polygon1 = new GMapPolygon(TwoPoints, "mypolygon");
                     polygon1.Fill = new SolidBrush(Color.FromArgb(50, 105, 105, 105));
                     polygon1.Stroke = new Pen(Color.FromArgb(105, 105, 105), 1);
                     polyOverlayBordersPolyChange.Polygons.Add(polygon1);
                     gMapControl1.Overlays.Add(polyOverlayBordersPolyChange);

                 }
             }

             if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Control) == Keys.Control)
             {
                 polygonHalfWeek = new GMapPolygon(pointsHalfWeek, "mypolygon");
                 //GMapOverlay polyOverlayChange = new GMapOverlay("polygons");
                 polygonHalfWeek.Fill = new SolidBrush(Color.FromArgb(10, 0, 255, 0));
                 polygonHalfWeek.Stroke = new Pen(Color.FromArgb(255, 0, 255, 0), 2);
                 polyOverlayChange.Polygons.Add(polygonHalfWeek);
                 gMapControl1.Overlays.Add(polyOverlayChange);

                 for (int i = 0; i < pointsHalfWeek.Count; i++)
                 {
                     xp.Add(pointsHalfWeek[i].Lat);
                     yp.Add(pointsHalfWeek[i].Lng);
                 }
                 InsidePolygonChangeSector(xp, yp);
                 NewChangeSector();
                 label9.Text = label15.Text = CountInDisrt.ToString();
                 CountInDisrt = 0;
                 for (int i = pointsHalfWeek.Count - 1; i >= 0; i--)
                     pointsHalfWeek.RemoveAt(i);

                 textBox1.Enabled = true;
                 textBox1.Text = "";
             }*/
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            foreach(Route rt in listRoutes)
            {
                if(rt.Team.Equals(comboBox1.Text) && rt.Region.Equals(comboBox2.Text))
                {
                    dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name,getPlan(rt.Name,rt.Region)+getFact(rt.CodeGPS));
                }
            }
        }

        private string getFact(string codeGPS)//проверка наличия Факта
        {
            string resStr = "";
            DateTime dt = dateTimePicker1.Value;
            DirName = dt.ToString("yyyy_MM_dd");
            if (!Directory.Exists(Constants.Path_fact))
            {
                MessageBox.Show("Отсутствует директория с фактическими маршрутами!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            DirectoryInfo info = new System.IO.DirectoryInfo(Constants.Path_fact);
            DirectoryInfo[] dirs = info.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                if (di.Name == DirName) //если есть папка с фактическими маршрутами
                {
                    FileInfo[] files = di.GetFiles();
                    foreach (FileInfo fi in files)//если найден файл с фактическим маршрутом на указанную дату
                    {
                        if(fi.Name.Contains(codeGPS))
                            resStr += "Ф";
                    }
                    
                    break;
                }
            }


            return resStr; ;
        }

        private string getPlan(string name, string branch)//проверка наличия Плана
        {
            string resStr = "";
            DateTime dt = dateTimePicker1.Value;
            DirName = dt.ToString("yyyy_MM_dd");
            if (!Directory.Exists(Constants.Path_plan))
            {
                MessageBox.Show("Отсутствует директория с плановыми маршрутами!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            DirectoryInfo info = new System.IO.DirectoryInfo(Constants.Path_plan);
            DirectoryInfo[] dirs = info.GetDirectories();
            bool isFolder = false;
            foreach (DirectoryInfo di in dirs)            
                if (di.Name == DirName) //если есть папка с плановыми маршрутами
                    isFolder = true;

            if (isFolder)
            {
                DirectoryInfo info1 = new System.IO.DirectoryInfo(Constants.Path_plan + "/" + DirName);
                DirectoryInfo[] dirs1 = info1.GetDirectories();
                foreach (DirectoryInfo di in dirs1)
                {
                    if(di.Name==branch)
                    {
                        FileInfo[] files = di.GetFiles();
                        foreach (FileInfo fi in files)//если найден файл с плановым маршрутом на указанную дату
                        {
                            if (fi.Name.Contains(name))
                                resStr += "П";
                        }

                        break;
                    }
                }
                   
            }            
            return resStr; ;
        }

        private List<double> GetResultForGrid(string PathFact, string PathPlan)
        {
            double WorkTime = 0;
            double WorkDist = 0;
            List<double> Result = new List<double>();
            DateTime Begin = DateTime.MinValue, End = DateTime.MinValue;
            List<FactPoints> FP = new List<FactPoints>();
            string[] Str = PathPlan.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] StrArray1;
            DateTime dt = DateTime.MinValue, dt_old = DateTime.MinValue, dt_old_last = DateTime.MinValue;
            bool ct = false;
            double Dist = 0, DistAll = 0;
            double DeltaTime = 0;//время остановки
            double latRoute = 0, lngRoute = 0, latRoute_old = 0, lngRoute_old = 0;
            try
            {
                StreamReader reader = new StreamReader(PathFact, System.Text.Encoding.Default);
                string s = "";
                while (true)
                {
                    s = reader.ReadLine();
                    if (s == null || s == "")
                        break;

                    StrArray1 = s.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    latRoute = Convert.ToDouble(StrArray1[1]) / 100 - (int)Convert.ToDouble(StrArray1[1]) / 100;
                    lngRoute = Convert.ToDouble(StrArray1[2]) / 100 - (int)Convert.ToDouble(StrArray1[2]) / 100;

                    latRoute = (int)Convert.ToDouble(StrArray1[1]) / 100 + latRoute * 100 / 60;
                    lngRoute = (int)Convert.ToDouble(StrArray1[2]) / 100 + lngRoute * 100 / 60;
                    if (ct)
                    {
                        dt = DateTime.ParseExact(StrArray1[0], "dd.MM.yyyy HH:mm.ss", CultureInfo.InvariantCulture).AddHours(3);
                        if (dt <= dt_old || dt <= dt_old_last)
                            continue;
                        End = dt;
                        Dist = Constants.getDistance(latRoute, lngRoute, latRoute_old, lngRoute_old);

                        DeltaTime = Constants.GetSeconds(dt) - Constants.GetSeconds(dt_old);
                        if (Dist < Constants.RADIUS_SMOOTH)
                        {
                            //DeltaTime = GetSeconds(dt) - GetSeconds(dt_old);
                            dt_old_last = dt;
                            continue;
                        }
                        else
                        {
                            if (DeltaTime > Constants.STOP_TIME_IN_POINT && Dist < Constants.CRITICAL_DISTANCE)
                            {
                                //FP.Add(new FactPoints(latRoute_old, lngRoute_old, dt_old, dt_old_last, DistAll));
                                FP.Add(new FactPoints(latRoute_old, lngRoute_old, dt_old, dt, DistAll));
                                DeltaTime = 0;
                            }
                        }
                        DistAll += Dist;

                    }
                    latRoute_old = latRoute;
                    lngRoute_old = lngRoute;
                    dt_old = DateTime.ParseExact(StrArray1[0], "dd.MM.yyyy HH:mm.ss", CultureInfo.InvariantCulture).AddHours(3);
                    if (!ct)
                        Begin = dt_old;
                    ct = true;
                }
            }
            catch (Exception ex) { }

            LFP.Add(Str[Str.Length - 1].Replace(".ltp", ""), FP);
            Result.Add(GetCountPlanPoint(PathPlan));
            if (PointsInRoute.ContainsKey(Str[Str.Length - 1].Replace(".ltp", "")))
            {
                Result.Add(GetVisitedPoints(PointsInRoute[Str[Str.Length - 1].Replace(".ltp", "")], LFP[Str[Str.Length - 1].Replace(".ltp", "")]));
                WorkTime = GetWorkTime(PointsInRoute[Str[Str.Length - 1].Replace(".ltp", "")]);
                WorkDist = GetWorkDist(PointsInRoute[Str[Str.Length - 1].Replace(".ltp", "")]);
            }
            else
                Result.Add(1);
            Result.Add(Math.Round((double)(Constants.GetSeconds(End) - Constants.GetSeconds(Begin)) / 3600, 2));
            Result.Add(WorkTime);
            Result.Add(Math.Round(DistAll / 1000, 3));
            Result.Add(WorkDist);
            return Result;
        }

        private double GetWorkTime(List<Point> pts)//расчет времени от посещения первой точки до последней
        {
            DateTime MinT = pts.Min(Point => Point.FactTimeArrive);
            DateTime MaxT = pts.Max(Point => Point.FactTimeDepart);
            double ResTime = Math.Round((double)(Constants.GetSeconds(MaxT) - Constants.GetSeconds(MinT)) / 3600, 2);
            if (ResTime > 0)
                return ResTime;
            else
                return 0;
        }

        public static double GetWorkDist(List<Point> pts)//расчет расстояния от посещения первой точки до последней
        {
            double MinD = 1000000;
            foreach (Point pt in pts)
            {
                if (pt.FactDistFromBeg < MinD && pt.FactDistFromBeg != 0)
                    MinD = pt.FactDistFromBeg;
            }
            double MaxD = pts.Max(Point => Point.FactDistFromBeg);
            double ResDist = Math.Round((MaxD - MinD) / 1000, 3);
            if (ResDist > 0)
                return ResDist;
            else
                return 0;
        }

        private int GetVisitedPoints(List<Point> Plan, List<FactPoints> Fact)
        {
            int CountVisitedPts = 0;
            foreach (FactPoints fp in Fact)
            {
                CountVisitedPts += fp.InsidePolygonFP(fp.SectorX, fp.SectorY, Plan);
            }
            return CountVisitedPts;
        }

        private int GetCountPlanPoint(string PathPlan)
        {
            string[] Str = PathPlan.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            List<Point> Points = new List<Point>();
            int CountPlanTP = 0;
            if (File.Exists(PathPlan))
            {
                //PointsInRoute = new Dictionary<string, List<Point>>();
                StreamReader reader = new StreamReader(PathPlan, System.Text.Encoding.Default);
                string s = "";
                while (true)
                {
                    s = reader.ReadLine();
                    if (s == null || s == "")
                        break;

                    string[] StrPlan = s.Split(new Char[] { '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (StrPlan[0] == "**")
                        continue;
                    if (StrPlan[0] == "p")
                    {
                        Point Pt = new Point(++CountPlanTP, StrPlan[1].ToString(), StrPlan[2].ToString(), Convert.ToDouble(StrPlan[3]), Convert.ToDouble(StrPlan[4]), Convert.ToDouble(StrPlan[5]));

                        Pt.SetDistTime(Convert.ToDouble(StrPlan[6]), Convert.ToDouble(StrPlan[7]), Convert.ToDouble(StrPlan[8]), Convert.ToDouble(StrPlan[9]));
                        Points.Add(Pt);

                    }
                }
                PointsInRoute.Add(Str[Str.Length - 1].Replace(".ltp", ""), Points);
                return Points.Count - 2;//точка выезда и приезда не учитывается
            }
            return 0;
        }
        //-----------------------------------------------

        private void FillGrid()//поиск в папке факт. маршрутов и запись из названий в грид
        {
            dataGridView1.Rows.Clear();
            //RouteCodeGPS.Clear();
            bool Find = false;
            DateTime dt = dateTimePicker1.Value;
            DirName = dt.ToString("yyyy_MM_dd");
            if (!Directory.Exists(Constants.Path_fact))
            {
                MessageBox.Show("Отсутствует директория с фактическими маршрутами!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DirectoryInfo info = new System.IO.DirectoryInfo(Constants.Path_fact);
            DirectoryInfo[] dirs = info.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                if (di.Name == DirName) //если есть папка с плановыми маршрутами
                {
                    Find = true;
                    //SetFactInGrid(DirName, GetPlanRoutes(dt));
                    break;
                }
            }

            /*if (!Find) //если на дату нет фактических маршрутов
            {
                List<string> ListPlan = GetPlanRoutes(dt);
                foreach (string s in ListPlan)
                {
                    int CountPlanPoint = GetCountPlanPoint(Constant.Path_plan + dateTimePicker1.Value.Month.ToString().PadLeft(2, '0') + "_" + dateTimePicker1.Value.Year.ToString() + "/" +
                       dateTimePicker1.Value.DayOfWeek.ToString() + "/" + comboBox3.Text + "/" + comboBox4.Text + "/" + s + ".ltp");
                    if (CountPlanPoint == 0)
                        dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, s, "--");
                    else
                        dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, s, "П", CountPlanPoint);
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
                }
            }*/
        }

        private List<string> GetPlanRoutes(DateTime dt)//получаем список названий плановых маршрутов
        {
            string id_branch = "";
            string id_user = "";
            List<int> ListRoutes = new List<int>();
            List<string> ListRoutesNames = new List<string>();

            SQLiteConnection m_dbConn = new SQLiteConnection();
            SQLiteCommand m_sqlCmd = new SQLiteCommand();
            m_dbConn = new SQLiteConnection("Data Source=" + dbFileNameAutorization + ";Version=3;");
            m_dbConn.Open();
            m_sqlCmd.Connection = m_dbConn;

           // m_sqlCmd.CommandText = "SELECT id FROM branches WHERE branch='" + comboBox3.Text + "'";
            id_branch = m_sqlCmd.ExecuteScalar().ToString();

            /*if (Login != "Admin")
            {
                m_sqlCmd.CommandText = "SELECT id FROM users WHERE login='" + Login + "'";
                id_user = m_sqlCmd.ExecuteScalar().ToString();

                m_sqlCmd.CommandText = "SELECT id_routes FROM result_table_auto WHERE id_user=" + id_user + " AND id_branch=" + id_branch + " AND team='" + comboBox4.Text + "'";
                SQLiteDataReader reader1 = m_sqlCmd.ExecuteReader();
                while (reader1.Read())
                {
                    ListRoutes.Add(Convert.ToInt32(reader1[0].ToString()));
                }
                reader1.Close();
                foreach (int num in ListRoutes)
                {
                    m_sqlCmd.CommandText = "SELECT route_name FROM routes WHERE id=" + num.ToString();
                    ListRoutesNames.Add(m_sqlCmd.ExecuteScalar().ToString());
                }
            }
            else
            {
                m_sqlCmd.CommandText = "SELECT route_name FROM routes WHERE id_branch=" + id_branch + " AND team='" + comboBox4.Text + "'";
                SQLiteDataReader reader1 = m_sqlCmd.ExecuteReader();
                while (reader1.Read())
                {
                    ListRoutesNames.Add(reader1[0].ToString());

                }
                reader1.Close();

            }
            m_dbConn.Close();
            return ListRoutesNames;*/
            return null;
        }

       /* private void SetFactInGrid(string DirName, List<string> PlanRoutes)//заносит данные в таблицу
        {
            List<double> ResParam = new List<double>();
            List<string> CodesGPS = new List<string>();
            CodesGPS = GetCodes(PlanRoutes);

            string PathPlan = Constant.Path_plan + dateTimePicker1.Value.Month.ToString().PadLeft(2, '0') + "_" + dateTimePicker1.Value.Year.ToString() +
                "/" + dateTimePicker1.Value.DayOfWeek + "/" + comboBox3.Text + "/" + comboBox4.Text + "/";
            int countRow = 0;
            DirectoryInfo info = new DirectoryInfo(Constants.Path_fact + "/" + DirName);
            FileInfo[] files = info.GetFiles();
            foreach (FileInfo fi in files)
            {
                if (!CodesGPS.Contains(fi.Name))//если данного имени файла нет в списке кодов разрешенных маршрутов, то дальше не идем 
                    continue;
                string s = GetFactRouteName(fi.Name);//в name указан код.gps
                if (s != null)
                {
                    ResParam = GetResultForGrid(Constant.Path_fact + "/" + DirName + "/" + fi.Name, PathPlan + s + ".ltp");
                    if (File.Exists(PathPlan + s + ".ltp"))
                    {
                        dataGridView1.Rows.Add(++countRow, s, "ФП", ResParam[0], ResParam[1], ResParam[2], ResParam[3], ResParam[4], ResParam[5]);
                        PlanRoutes.Remove(s);
                    }
                    else
                    {
                        dataGridView1.Rows.Add(++countRow, s, "Ф", "", "", ResParam[2], "", ResParam[4]);
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
                        PlanRoutes.Remove(s);
                    }
                }
            }

            foreach (string s in PlanRoutes)
            {
                int NumPlanPoints = GetCountPlanPoint(PathPlan + s + ".ltp");
                if (NumPlanPoints == 0)
                    dataGridView1.Rows.Add(++countRow, s, "--");
                else
                    dataGridView1.Rows.Add(++countRow, s, "П", NumPlanPoints);
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
            }

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (!toolStripButton4.Checked)
            {
                SumDist = 0;
                StartFinish = false;
                TwoPointDist.Clear();
                CountPts = 0;
                markersOverlayStartFin.Clear();
                gMapControl1.Overlays.Clear();
                gMapControl1.Zoom += 0.1;
                gMapControl1.Zoom -= 0.1;
            }
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
