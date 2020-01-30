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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class Form1 : Form
    {
        List<Route> listRoutes = new List<Route>();
        List<Point> TradePoints = new List<Point>();
        string DirName = "";       
        GMapOverlay markersOverlay = new GMapOverlay("marker");

        GMapOverlay markersOverlayStartFin = new GMapOverlay("marker");
        bool StartFinish = false;
        double FirstLat = 0, FirstLng = 0;
        List<PointLatLng> TwoPointDist = new List<PointLatLng>();
        double SumDist = 0;
        int CountPts = 1;
        public Dictionary<string, List<Point>> PointsInRoute = new Dictionary<string, List<Point>>();
        public Dictionary<string, List<FactPoints>> LFP = new Dictionary<string, List<FactPoints>>();


        //----для области-----------------
        GMapPolygon polygonHalfWeek;
        List<PointLatLng> pointsHalfWeek = new List<PointLatLng>();
        //List<Elementary_route> List_ER = new List<Elementary_route>();
        GMapOverlay polyOverlayRoute = new GMapOverlay("polygons");
        GMapOverlay polyOverlayBordersPoly = new GMapOverlay("polygons");
        GMapOverlay polyOverlayBordersPolyChange = new GMapOverlay("polygons");
        GMapOverlay polyOverlayChange = new GMapOverlay("polygons");
        GMapOverlay polyBorderRoute = new GMapOverlay("polygons");
        List<Point> ChangeSector = new List<Point>();
        int CountInDisrt = 0;

        public Form1()
        {
            InitializeComponent();
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            Logger.Log.Info("Main form start");
            listRoutes = DBHandler.GetListRoutes();//получение всех маршрутов, которые есть в системе
            SetComboBox();
        }        

        private void SetComboBox()
        {
            Logger.Log.Info("SetComboBox");
            foreach (Route rt in listRoutes)
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
            Logger.Log.Info("Выбрано Google карту");
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void спутниковаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано спутниковую Google карту");
            gMapControl1.MapProvider = GoogleSatelliteMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано OSM карту");
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
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(48.489, 34.993);
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;
            gMapControl1.DragButton = MouseButtons.Left;
            router = new Router(routerDb);
            Logger.Log.Info("Form1 загружена");
        }

        
        private void gMapControl1_OnMapZoomChanged()
        {
            trackBar1.Value = (int)gMapControl1.Zoom;
        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            //определение расстояния по прямой

            List<double> xp = new List<double>();
            List<double> yp = new List<double>();
            
            if (toolStripButton6.Checked)
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
            else
            {

                if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    if (ChangeSector.Count > 0)
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
                    label7.Text = CountInDisrt.ToString();
                    label11.Text = CountInDisrt.ToString();
                    CountInDisrt = 0;
                    for (int i = pointsHalfWeek.Count - 1; i >= 0; i--)
                        pointsHalfWeek.RemoveAt(i);

                }
            }
        }

        private void NewChangeSector()
        {
            ChangeSector.Reverse();
            for (int i = 0; i < ChangeSector.Count; i++)
                for (int j = i + 1; j < ChangeSector.Count; j++)
                {
                    if (ChangeSector[i].CodeTradePoint == ChangeSector[j].CodeTradePoint)
                    {
                        ChangeSector.RemoveAt(j);
                        j--;
                    }
                }
        }

        public void InsidePolygonChangeSector(List<double> xp, List<double> yp)
        {
            try
            {
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
                        bool IsDouble = false;
                        for (int i = 0; i < ChangeSector.Count; i++)
                        {
                            if (ChangeSector[i].CodeTradePoint == TrPoint.CodeTradePoint)
                            {
                                IsDouble = true;
                                break;
                            }
                        }
                        if (!IsDouble)
                            CountInDisrt++;

                        ChangeSector.Add(TrPoint);//содержит точки из выделенной области
                    }
                }
            }
            catch
            {
                return;
            }
        }
        
        //--------------------Паналь 1, просмотр плана/факта-------------------
        private void button1_Click_1(object sender, EventArgs e)//Вывод маршрутов в таблицу
        {
            this.Cursor = Cursors.WaitCursor;
            dataGridView1.Rows.Clear();
            PointsInRoute.Clear();
            LFP.Clear();
            DateTime dt = dateTimePicker1.Value;
            DirName = dt.ToString("yyyy_MM_dd");

            foreach (Route rt in listRoutes)
            {
                if(rt.Team.Equals(comboBox1.Text) && rt.Region.Equals(comboBox2.Text))
                {
                    List<double> ResParam = new List<double>();
                    ResParam = GetResultForGrid(Constants.Path_fact + "/" + DirName + "/" + rt.CodeGPS+".gps", 
                        Constants.Path_plan + "/" + DirName + "/" +rt.Region+"/"+rt.Name+ ".ltp");

                    string plan = getPlan(rt.Name, rt.Region);
                    string fact = getFact(rt.CodeGPS);
                    if(plan=="П" && fact=="Ф")
                        dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name,plan+"/"+fact,
                            ResParam[0], ResParam[1], ResParam[2], ResParam[3], ResParam[4], ResParam[5]);
                    else
                    {
                        if (plan == "П" && fact != "Ф")
                        {
                            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name, plan + "/-",
                            ResParam[0], ResParam[1], "", "", "", "");
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
                        }
                        if (plan != "П" && fact == "Ф")
                        {
                            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name, "-/" + fact,
                            ResParam[0], ResParam[1], ResParam[2], "", ResParam[4], "");
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
                        }
                        
                    }

                    Logger.Log.Info("Маршрут "+rt.Name+" "+plan+fact);
                }

               
            }
            this.Cursor = Cursors.Default;
        }

        private string getFact(string codeGPS)//проверка наличия Факта
        {
            Logger.Log.Info("Функция getFact()");
            string resStr = "";
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
            Logger.Log.Info("Функция getPlan()");
            string resStr = "";
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
                            if (fi.Name==name+".ltp")
                                resStr += "П";
                        }

                        break;
                    }
                }
                   
            }            
            return resStr; ;
        }

        private List<double> GetResultForGrid(string PathFact, string PathPlan)//функция для расчета основных параметров плана и факта
        {
            Logger.Log.Info("Функция GetResultForGrid()");
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
            Logger.Log.Info("Функция GetWorkTime()");
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
            Logger.Log.Info("Функция GetWorkDist()");
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
            //подсчет количества посещенных точек, которые попадают в радиус остановки торгового представителя
            Logger.Log.Info("Функция GetVisitedPoints()");
            int CountVisitedPts = 0;
            foreach (FactPoints fp in Fact)
            {
                CountVisitedPts += fp.InsidePolygonFP(fp.SectorX, fp.SectorY, Plan);
            }
            return CountVisitedPts;
        }

        private int GetCountPlanPoint(string PathPlan)
        {
            //расчет количества плановых точек посещения
            Logger.Log.Info("Функция GetCountPlanPoint()");
            string[] Str = PathPlan.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            List<Point> Points = new List<Point>();
            int CountPlanTP = 0;
            if (File.Exists(PathPlan))
            {
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //отрисовка планового и фактического маршрута при клике на строке таблицы
            Logger.Log.Info("Функция GetCountPlanPoint()");
            try { DrawPlanRoute(); } catch (Exception ex) { }
            

            List<PointLatLng> FalseRoute = new List<PointLatLng>();
            List<FactPoints> LFP1 = new List<FactPoints>();
            List<GMap.NET.PointLatLng> listGPS = new List<GMap.NET.PointLatLng>();
            string[] StrArray1;
            DateTime dt = DateTime.MinValue, dt_old = DateTime.MinValue, dt_old_last = DateTime.MinValue, dt_first = DateTime.MinValue;
            bool ct = false;
            double Dist = 0;
            double DeltaTime = 0;//время остановки
            double latRoute = 0, lngRoute = 0, latRoute_old = 0, lngRoute_old = 0;
            try
            {
                StreamReader reader = null;
                foreach (Route rte in listRoutes)
                {
                    if(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString()==rte.Name)
                    {
                        reader = new StreamReader(Constants.Path_fact + "/" + DirName + "/" + rte.CodeGPS+".gps", Encoding.Default);
                        break;
                    }
                }
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
                        Dist = Constants.getDistance(latRoute, lngRoute, latRoute_old, lngRoute_old);
                        DeltaTime = Constants.GetSeconds(dt) - Constants.GetSeconds(dt_old);

                        if (Dist < Constants.RADIUS_SMOOTH)
                        {
                            dt_old_last = dt;
                            continue;
                        }
                        else
                        {
                            if (DeltaTime > Constants.STOP_TIME_IN_POINT && Dist < Constants.CRITICAL_DISTANCE)
                            {
                                LFP1.Add(new FactPoints(latRoute_old, lngRoute_old, dt_old, dt, 0));
                                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(latRoute_old, lngRoute_old), new Bitmap("./images/stop.png"));// GMarkerGoogleType.red_small);
                                marker.ToolTip = new GMapRoundedToolTip(marker);
                                marker.ToolTipText = "Остановка " + Math.Round((DeltaTime / 60), 1).ToString() + " мин\n(" + dt_old.ToLongTimeString() + "-" + dt.ToLongTimeString() + ")";
                                markersOverlay.Markers.Add(marker);
                                DeltaTime = 0;


                            }

                            if (Constants.GetSeconds(dt) - Constants.GetSeconds(dt_old_last) > 60)
                            {
                                FalseRoute.Add(new PointLatLng(latRoute_old, lngRoute_old));
                                FalseRoute.Add(new PointLatLng(latRoute, lngRoute));
                            }
                        }

                    }
                    latRoute_old = latRoute;
                    lngRoute_old = lngRoute;
                    dt_old = DateTime.ParseExact(StrArray1[0], "dd.MM.yyyy HH:mm.ss", CultureInfo.InvariantCulture).AddHours(3);
                    dt_old_last = dt_old;
                    
                    listGPS.Add(new PointLatLng(latRoute, lngRoute));
                    if (!ct)
                    {
                       GMarkerGoogle marker2 = new GMarkerGoogle(new PointLatLng(latRoute_old, lngRoute_old), new Bitmap("./images/man_begin.png"));
                        marker2.ToolTip = new GMapRoundedToolTip(marker2);
                        marker2.ToolTipText = "Начало маршрута " + dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString() + "\n" +
                            dt_old.ToLongTimeString();
                        markersOverlay.Markers.Add(marker2);

                    }
                    ct = true;

                }
                GMarkerGoogle marker1 = new GMarkerGoogle(new PointLatLng(latRoute_old, lngRoute_old), new Bitmap("./images/man.png"));
                marker1.ToolTip = new GMapRoundedToolTip(marker1);
                marker1.ToolTipText = "Конец маршрута " + dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString() + "\n" +
                    dt.ToLongTimeString();
                markersOverlay.Markers.Add(marker1);

                GMapRoute r = new GMapRoute(listGPS, "Route");
                r.IsVisible = true;
                r.Stroke = new Pen(Color.Red, 2);
                markersOverlay.Routes.Add(r);
                gMapControl1.Overlays.Add(markersOverlay);

                DrawCircle(LFP1);
                gMapControl1.Zoom += 0.1;
                gMapControl1.Zoom -= 0.1;
                gMapControl1.Position = new PointLatLng(latRoute, lngRoute);

                DrawFalseRoutes(FalseRoute);
                if (!Constants.firstDraw)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.WindowState = FormWindowState.Maximized;
                    Constants.firstDraw = true;
                }
            }
            catch (Exception ex)
            { }
        }

        private void DrawCircle(List<FactPoints> fps)
        {
            //отрисовка окружностей при остановке торгового агента для визуального контроля посещения торговой точки
            Logger.Log.Info("Функция DrawCircle()");
            foreach (FactPoints fp in fps)
            {
                List<PointLatLng> Poly = new List<PointLatLng>();
                for (int i = 0; i < fp.SectorX.Count; i++)
                {
                    Poly.Add(new PointLatLng(fp.SectorX[i], fp.SectorY[i]));
                }
                GMapPolygon polygonNet = new GMapPolygon(Poly, "mypolygon");
                GMapOverlay polyOverlayNET = new GMapOverlay("polygons");
                polygonNet.Fill = new SolidBrush(Color.FromArgb(40, 0, 255, 255));
                polygonNet.Stroke = new Pen(Color.FromArgb(255, 0, 0, 255), 1);
                polyOverlayNET.Polygons.Add(polygonNet);
                gMapControl1.Overlays.Add(polyOverlayNET);
            }
        }

        private void DrawFalseRoutes(List<PointLatLng> FalseRoute)
        {
            //Отрисовка прямой пунктирной линии при отсутствии данных от gps
            Logger.Log.Info("Функция DrawFalseRoutes()"); 
            for (int i = 0; i < FalseRoute.Count; i = i + 2)
            {
                List<PointLatLng> Line = new List<PointLatLng>();
                Line.Add(FalseRoute[i]);
                Line.Add(FalseRoute[i + 1]);

                GMapRoute r = new GMapRoute(Line, "Route");
                r.IsVisible = true;
                r.Stroke = new Pen(Color.WhiteSmoke, 2);
                r.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                markersOverlay.Routes.Add(r);
            }
            gMapControl1.Overlays.Add(markersOverlay);
        }

        private void DrawPlanRoute()
        { //Отрисовка планового маршрута зеленого цвета
            GMapOverlay markersOverlay1 = new GMapOverlay("marker1");
            string FilePath = Constants.Path_plan + DirName + "/" + comboBox2.Text +
                "/" + dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString() + ".ltp";

            List<PointLatLng> RoutePlan = Route.getPlanRoute(FilePath);

            foreach (Point p in PointsInRoute[dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value.ToString()])
            {
                GMarkerGoogle marker = null;
                if (p.Adress == "Start")
                {
                    marker = new GMarkerGoogle(new PointLatLng(p.X, p.Y), GMarkerGoogleType.purple_dot);
                    marker.ToolTip = new GMapRoundedToolTip(marker);
                    marker.ToolTipText = "Начальная точка";
                    markersOverlay1.Markers.Add(marker);
                    continue;
                }

                if (p.IsVisited)
                    marker = new GMarkerGoogle(new PointLatLng(p.X, p.Y), GMarkerGoogleType.gray_small);
                else
                    marker = new GMarkerGoogle(new PointLatLng(p.X, p.Y), GMarkerGoogleType.yellow_small);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTipText = p.CodeTradePoint + "_" + p.Adress;
                markersOverlay1.Markers.Add(marker);
                TradePoints.Add(p);
            }

            GMapRoute r = new GMapRoute(RoutePlan, "RoutePlan");
            r.IsVisible = true;
            r.Stroke = new Pen(Color.DarkGreen, 3);
            markersOverlay.Routes.Add(r);
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(markersOverlay1);

            label2.Text = TradePoints.Count.ToString();
            gMapControl1.Zoom += 0.1;
            gMapControl1.Zoom -= 0.1;
            gMapControl1.Position = new PointLatLng(TradePoints[TradePoints.Count - 1].X, TradePoints[TradePoints.Count - 1].Y);
        }        

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            Logger.Log.Info("Функция dataGridView1_DoubleClick()");
            try
            {
                string Name = dataGridView1.Rows[dataGridView1.CurrentCellAddress.Y].Cells[1].Value.ToString();
                PointsInRoute PR = new PointsInRoute(comboBox1.Text, Name, PointsInRoute[Name], dateTimePicker1.Value);
                PR.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("По данному маршруту отсутствуют плановые точки!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log.Info("Отсутствуют плановые точки!");
            }
        }

        //--------------------Панель 1 вверх, просмотр плана/факта-------------------
        //-------------------Панель 2, расчет маршрута
        bool MakeRoute = false;//true - если строим окончательный маршрут - не очищаем
        float DeltaXOSM = 0, DeltaYOSM = 0;
        Region Reg;
        RouterDb routerDb = new RouterDb();
        Router router;
        String dbFileName = "";
        string[] StrArray;
        List<string> RoutesName = new List<string>();//список названий маршрутов
        int clr = 0; // номер цвета точки
        int GlobalCount = 0;
        int CountNumDay = 0;//порядковый номер дня недели
        bool First = true;
        List<Point>[] PointsOfRoute;
        List<Point> PointSector = new List<Point>();//точки в секторе в порядке уменьшения расстояния к центру
        List<Point> PointSectorOpt = new List<Point>();
        List<Point>[] PointSectorOptRes;
        bool ErrorOSM = false;//становится true, если OSM не находит координаты
        List<Point> LastNodesInSec = new List<Point>();
        string[] NameDay = { "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ", "ВС" };
        GMarkerGoogleType[] col = { GMarkerGoogleType.green_small, GMarkerGoogleType.yellow_small,
                                  GMarkerGoogleType.red_small,GMarkerGoogleType.blue_small,
                                  GMarkerGoogleType.brown_small,GMarkerGoogleType.white_small,
                                  GMarkerGoogleType.purple_small,GMarkerGoogleType.orange_small,
                                  GMarkerGoogleType.gray_small, GMarkerGoogleType.black_small
                                   };
        int CountCZ = 1;//суммарное количество машино-зон

        private void button2_Click(object sender, EventArgs e)
        {
            gMapControl1.Overlays.Clear();
            string Brnd = "";//наименование бренда
            string Route = "";//наименование маршрута

            if (comboBox3.Enabled && radioButton2.Checked)
            {
                MessageBox.Show("Выберите филиал из выпадающего списка!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            openFileDialog1.Filter = "Text Documents (*.txt)|*.txt|All Files|*.*";
            openFileDialog1.FileName = "Trade Points";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = new StreamReader(openFileDialog1.FileName, Encoding.Default);
                string s = "";
                while (true)
                {
                    s = reader.ReadLine();
                    if (s == null || s == "")
                        break;

                    StrArray = s.Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (StrArray[0] == "**")
                        continue;

                    if (StrArray[0] == "@")
                    {
                        Brnd = StrArray[1];
                        continue;
                    }

                    if (StrArray[0] == "Rt")
                    {
                        try
                        {
                            Route = StrArray[1];
                            RoutesName.Add(StrArray[1]);
                        }
                        catch (Exception ex) { Route = "NO"; }

                        //if (radioButton2.Checked)//чтобы маршруты отображались разными цветами
                        {
                            clr++;
                            if (clr > 9)
                                clr = 0;
                        }

                        continue;
                    }

                    try
                    {
                        Point Pt = null;
                        
                            Pt = new Point(++GlobalCount, StrArray[1].ToString(), StrArray[2].ToString(), Convert.ToDouble(StrArray[3]), Convert.ToDouble(StrArray[4]), Convert.ToDouble(StrArray[5]));

                        if (CountNumDay == 7)
                            CountNumDay = 0;
                        Pt.AddParam(Brnd, Route, NameDay[CountNumDay], clr);
                        TradePoints.Add(Pt);
                    }
                    catch
                    {
                        MessageBox.Show("Проверьте правильность ввода исходных данных!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                }
                if (CountNumDay == 0 && First)
                {
                    Reg = new Region(TradePoints);
                    First = false;
                }
                Reg.OpenFileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                this.Text = Constants.NameProg + " (" + Reg.OpenFileName + ")";
                label8.Text = Reg.TradePoints.Count.ToString();//общее количество ТТ

                //button1.Enabled = true;
                //button3.Enabled = true;

                AddMarkersToControl();
                gMapControl1.Position = new PointLatLng(TradePoints[TradePoints.Count - 1].X, TradePoints[TradePoints.Count - 1].Y);

                if (!radioButton2.Checked)
                    clr++;
                CountNumDay++;
            }
            else
                return;
        }

        private void AddMarkersToControl() //добавляет маркеры на карту
        {

            for (int i = 0; i < TradePoints.Count; i++)
            {
                GMarkerGoogle marker = null;
                if (TradePoints[i].CodeTradePoint == "Start")
                    marker = new GMarkerGoogle(new PointLatLng(TradePoints[i].X, TradePoints[i].Y), GMarkerGoogleType.pink_dot);
                else
                    marker = new GMarkerGoogle(new PointLatLng(TradePoints[i].X, TradePoints[i].Y), col[TradePoints[i].Color]);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTipText = TradePoints[i].Number.ToString() + "_" + TradePoints[i].CodeTradePoint + "_" + TradePoints[i].Adress;
                markersOverlay.Markers.Add(marker);
            }

            gMapControl1.Overlays.Add(markersOverlay);
        }

        private void AddInComboRoutes()
        {
            try
            {
                comboBox4.Enabled = true;
                foreach (string s in RoutesName)
                {
                    comboBox4.Items.Add(s);
                }
                comboBox4.Text = RoutesName[0];
            }
            catch
            {
                MessageBox.Show("Ошибка в файле модели!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            AddInComboRoutes();
            PointsOfRoute = new List<Point>[RoutesName.Count];
            PointSectorOptRes = new List<Point>[RoutesName.Count];
            for (int i = 0; i < RoutesName.Count; i++)
            {
                PointsOfRoute[i] = new List<Point>();
                PointSectorOptRes[i] = new List<Point>();
            }
            foreach (Point p in Reg.TradePoints)//добавление точек в списки маршрутов
            {
                PointsOfRoute[RoutesName.IndexOf(p.RouteName)].Add(p);
            }

            for (int sr = 0; sr < RoutesName.Count; sr++)
            {

                PointSector.Clear();
                PointSectorOpt.Clear();
                //PointSectorOptRes.Clear();
                foreach (Point p in Reg.TradePoints)
                {
                    if (p.RouteName == RoutesName[sr])
                        PointSector.Add(p);
                }

                List<Point> TimeListPoint = new List<Point>();
                int k = 0;
                string Brnd = PointSector[1].Brand;
                bool All = false;
                string StrResult = "";
                for (int i = 0; i < PointSector.Count; i++)
                {
                    if (PointSector[i].Adress.Contains(Brnd) || PointSector[i].Adress == "Start")
                    {
                        TimeListPoint.Add(PointSector[i]);
                        All = true;
                    }
                    else
                    {
                        All = false;
                        k = i;
                        if (i > 0)
                            break;
                    }
                }

                do
                {
                    Reg.FirstEndTime = 0;
                    double[,] Arr = new double[TimeListPoint.Count, TimeListPoint.Count];
                    Arr = DistancesOSM(TimeListPoint);

                    Reg.SetMainTable(Arr);//сохраняем исходную таблицу
                    Reg.ResString = "";
                    Reg.EndResString = "";
                    Reg.EndResTime = Constants.EndRt;
                    StrResult = Reg.OptimalRouteMain(Arr);

                    Reg.ResolveFirstEnd(StrResult);

                    if (All)
                        break;
                    if (k == PointSector.Count)
                        break;
                    // if (Reg.AllDistRes - Reg.FirstEndTime > Convert.ToInt32(textBox11.Text))
                    // TimeListPoint.RemoveAt(TimeListPoint.Count - 1);
                    //else
                    if (Reg.FirstEndTime > Convert.ToInt32(textBox11.Text))
                        break;
                    TimeListPoint.Add(PointSector[k]);
                    k++;



                    //} while (Reg.FirstEndTime < Convert.ToInt32(textBox11.Text));
                } while (true);

                //TimeListPoint.RemoveAt(TimeListPoint.Count - 2);
                WriteInitialInFile(TimeListPoint);//запись исходных данных с учетом сектора и машино-зоны
                TimeListPoint = Order(StrResult, TimeListPoint);
                //if(!All)
                // TimeListPoint.RemoveAt(TimeListPoint.Count - 2);

                for (int t = 0; t < TimeListPoint.Count; t++)
                {
                    PointSectorOptRes[sr].Add(TimeListPoint[t]);
                }
                DrawPolylines(PointSectorOptRes[sr]);//отрисовка векторного маршрута

            }
            button4.Enabled = true;
            gMapControl1.Zoom += 0.1;
            gMapControl1.Zoom -= 0.1;
            this.Cursor = Cursors.Default;
        }

        private void DrawPolylines(List<Point> PointsCZ)
        {
            //polyOverlayRoute.Clear();
            markersOverlay.Routes.Clear();
            for (int i = 0; i < PointsCZ.Count - 1; i++)
            {
                List<PointLatLng> points = new List<PointLatLng>();
                try
                {
                    points.Add(new PointLatLng(PointsCZ[i].X, PointsCZ[i].Y));
                    points.Add(new PointLatLng(PointsCZ[i + 1].X, PointsCZ[i + 1].Y));
                }
                catch { }
                GMapPolygon polygon = new GMapPolygon(points, "mypolygon");
                polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
                polygon.Stroke = new Pen(Color.Red, 1);
                polyOverlayRoute.Polygons.Add(polygon);
            }
            gMapControl1.Overlays.Add(polyOverlayRoute);
        }

        private List<Point> Order(string ResStr, List<Point> ListPnts)//располагает точки в списке в порядке, указанном в ResStr
        {
            List<Point> OrderL = new List<Point>();

            int c = 0;
            for (int i = 0; i < ListPnts.Count; i++)
            {
                if (ListPnts[i].CodeTradePoint == "Start")
                {
                    OrderL.Add(ListPnts[i]);
                    c = i;
                    break;
                }
            }

            string[] ArrBranches = ResStr.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < ArrBranches.Length; i++)
            {
                for (int j = 0; j < ArrBranches.Length; j++)
                {
                    string[] Br = ArrBranches[j].Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Convert.ToInt32(Br[0]) == (c + 1))
                    {
                        OrderL.Add(ListPnts[Convert.ToInt32(Br[1]) - 1]);
                        c = Convert.ToInt32(Br[1]) - 1;
                        break;
                    }
                }
            }

            return OrderL;
        }

        private void WriteInitialInFile(List<Point> LPnts)//запись исходных данных в файл с учетом разбивки на сектора и машино-зоны
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("./NewRoutes/" + Reg.OpenFileName + "_data.txt", true, Encoding.UTF8))
                {
                    int k = 1;
                    sw.WriteLine("Rt\t" + LPnts[0].RouteName);
                    foreach (Point p in LPnts)
                    {
                        sw.WriteLine(k++.ToString() + "\t" + p.CodeTradePoint + "\t" +
                                    p.Adress + "\t" + Math.Round(p.X, 7).ToString() + "\t" + Math.Round(p.Y, 7).ToString() + "\t" +
                                    Math.Round(p.Time * 60).ToString());
                    }
                    sw.Close();
                }
                //MessageBox.Show("Файл результатов Result/" + Reg.OpenFileName + "_route.txt успешно записан","",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Файл результатов NewRoutes/" + Reg.OpenFileName + "_data.txt не записан", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public double[,] DistancesOSM(List<Point> PointsCarZone)
        {
            double[,] DistArray = new double[PointsCarZone.Count, PointsCarZone.Count];
            for (int i = 0; i < PointsCarZone.Count; i++)
            {
                for (int j = i; j < PointsCarZone.Count; j++)
                {
                    if (i == j)
                        DistArray[i, j] = Constants.EmptyCell;
                    else
                    {
                        double DistPointOSM = GetDistRouteOSM(PointsCarZone[i], PointsCarZone[j], true);

                        int iter = 0;
                        while (ErrorOSM)
                        {
                            if (iter > Constants.MaxIterations)
                            {
                                //DistPointOSM = GetDistRouteGoogle(PointsCarZone[i], PointsCarZone[j]);
                                break;
                            }
                            CorrectDeltaOSM(++iter);//уточнение расположения точки для возможности расчета расстояния
                            DistPointOSM = GetDistRouteOSM(PointsCarZone[i], PointsCarZone[j], true);//расчет расстояния между точками
                        }
                        if (iter > Constants.MaxIterations && DistPointOSM == 0)
                        {
                            MessageBox.Show("Перепривяжите точку " + (j + 1).ToString() + " код " + PointsCarZone[j].CodeTradePoint);
                            return DistArray;
                        }
                        DistArray[j, i] = DistArray[i, j] = DistPointOSM;
                    }

                }
            }
            return DistArray;
        }

        private void CorrectDeltaOSM(int iter)
        {
            switch (iter % 8)
            {
                case 1:
                    DeltaXOSM = 0;
                    DeltaYOSM = (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                case 2:
                    DeltaXOSM = (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                case 3:
                    DeltaXOSM = (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = 0;
                    break;
                case 4:
                    DeltaXOSM = (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = -1 * (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                case 5:
                    DeltaXOSM = 0;
                    DeltaYOSM = -1 * (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                case 6:
                    DeltaXOSM = -1 * (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = -1 * (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                case 7:
                    DeltaXOSM = -1 * (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = 0;
                    break;
                case 8:
                    DeltaXOSM = -1 * (float)(Constants.MinDeltaXOSM + Constants.MinDeltaXOSM * (int)(iter / 8));
                    DeltaYOSM = (float)(Constants.MinDeltaYOSM + Constants.MinDeltaYOSM * (int)(iter / 8));
                    break;
                default:
                    break;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ReDrawPoints();
            Constants.WorkTime = Convert.ToDouble(textBox11.Text);
            for (int z = 0; z < RoutesName.Count; z++)
            {
                StreamWriter sw = new StreamWriter(RoutesName[z].Trim() + ".ltp", true, Encoding.Default);
                sw.WriteLine("**\t" + RoutesName[z].Trim());
                sw.Close();
                CalculationRoute(z);

            }
            this.Cursor = Cursors.Default;
            WriteResInFile();//запись в файл данных про маршруты
            
        }

        private void WriteResInFile()//запись результатов в файл
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("./Result/" + Reg.OpenFileName + "_route.txt", false, Encoding.Default))
                {
                    sw.WriteLine("№ п/п\t№ ТТ\tКод ТТ\t\t\tSтт,км\tS,км\tТобщ,ч\tТ пр\tТ отпр");
                    for (int x = 0; x < RoutesName.Count; x++)
                    {
                        double Tm = 0;
                        int k = 1;
                        sw.WriteLine();
                        sw.WriteLine("-------------- " + RoutesName[x] + " --------------");
                        foreach (Point p in PointSectorOptRes[x])
                        {
                            if (k == 2)
                            {
                                Tm = p.TimeArrive;
                            }
                            if (k == 1)
                                sw.WriteLine(k++.ToString() + "\t" + p.Number + "\t" + p.CodeTradePoint + "\t\t\t0\t0\t-\t-\t" + TimeFormat(p.TimeDepart));
                            else
                            {
                                if (k != PointSectorOptRes[x].Count)
                                    sw.WriteLine(k++.ToString() + "\t" + p.Number + "\t" + p.CodeTradePoint + "\t" + p.DistPrevTP.ToString() + "\t" +
                                p.DistanceFromFirst.ToString() + "\t" + Math.Round((p.TimeDepart - Tm), 2).ToString() + "\t" + TimeFormat(p.TimeArrive) +
                                "\t" + TimeFormat(p.TimeDepart) + "\t" + p.Adress);
                                else
                                    sw.WriteLine(k++.ToString() + "\t" + p.Number + "\tEnd\t\t\t" + p.DistPrevTP.ToString() + "\t" +
                                    p.DistanceFromFirst.ToString() + "\t-\t" + TimeFormat(p.TimeArrive) + "\t-");
                            }
                        }

                    }
                    sw.Close();
                }
                //MessageBox.Show("Файл результатов Result/" + Reg.OpenFileName + "_route.txt успешно записан","",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Файл результатов Result/" + Reg.OpenFileName + "_route.txt не записан", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MessageBox.Show("Файл результатов Result/" + Reg.OpenFileName + "_route.txt успешно записан", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static string TimeFormat(double Hours)//переводит дробную часть часа в часы и минуты
        {
            string ResTime = "";
            double TimeArr = Hours;
            int Hour = (int)TimeArr;
            int Minute = (int)Math.Round((TimeArr - Hour) * 60);

            if (Minute >= 60)
            {
                Hour++;
                Minute -= 60;
            }

            ResTime = Hour.ToString() + ":" + Minute.ToString();

            if (Hour < 10 && Minute < 10)
                ResTime = "0" + Hour.ToString() + ":0" + Minute.ToString();
            if (Hour >= 10 && Minute < 10)
                ResTime = Hour.ToString() + ":0" + Minute.ToString();
            if (Hour < 10 && Minute >= 10)
                ResTime = "0" + Hour.ToString() + ":" + Minute.ToString();
            return ResTime;
        }

        private void ReDrawPoints()
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
        }

        private void CalculationRoute(int z)//расчитывает маршрут в пределах сектора
        {
            double TimeDepfirst = 0;
            if (PointSectorOptRes[z].Count > 1)
            {
                DeleteDouble(PointSectorOptRes[z]);
                MakeRoute = true;
                for (int i = 0; i < PointSectorOptRes[z].Count - 1; i++)
                {
                    double DistPointOSM = GetDistRouteOSM(PointSectorOptRes[z][i], PointSectorOptRes[z][i + 1], false);

                    int iter = 0;
                    while (ErrorOSM)
                    {
                        if (iter > Constants.MaxIterations)
                            break;
                        CorrectDeltaOSM(++iter);//уточнение расположения точки для возможности расчета расстояния
                        DistPointOSM = GetDistRouteOSM(PointSectorOptRes[z][i], PointSectorOptRes[z][i + 1], false);//расчет расстояния между точками
                    }

                    PointSectorOptRes[z][i + 1].DistPrevTP = DistPointOSM;

                    PointSectorOptRes[z][i + 1].DistanceFromFirst = PointSectorOptRes[z][i].DistanceFromFirst + PointSectorOptRes[z][i + 1].DistPrevTP;
                    double Speed = 0;
                    if (PointSectorOptRes[z][i + 1].DistPrevTP < 0.3)
                        Speed = Constants.SpeedFoot;
                    else
                    {
                        if (PointSectorOptRes[z][i + 1].DistPrevTP < 5)
                            Speed = Constants.SpeedCar;
                        else
                            Speed = Constants.SpeedCarHW;
                    }

                    if (i == 0)
                    {
                        PointSectorOptRes[z][i + 1].TimeArrive = 8;
                        PointSectorOptRes[z][i + 1].TimeDepart = PointSectorOptRes[z][i + 1].TimeArrive + PointSectorOptRes[z][i + 1].Time;
                        TimeDepfirst = Math.Round(PointSectorOptRes[z][i + 1].TimeArrive - Math.Round(PointSectorOptRes[z][i + 1].DistPrevTP / Speed, 2), 2);
                    }
                    else
                    {
                        PointSectorOptRes[z][i + 1].TimeArrive = PointSectorOptRes[z][i].TimeDepart +
                           Math.Round(PointSectorOptRes[z][i + 1].DistPrevTP / Speed, 2);
                        PointSectorOptRes[z][i + 1].TimeDepart = PointSectorOptRes[z][i].TimeDepart +
                            Math.Round(PointSectorOptRes[z][i + 1].DistPrevTP / Speed, 2) + PointSectorOptRes[z][i + 1].Time;

                    }

                    PointSectorOptRes[z][i].InRoute = true;
                    PointSectorOptRes[z][i + 1].InRoute = true;
                    PointSectorOptRes[z][i].CarZone = CountCZ;

                    if ((i + 1) == (PointSectorOptRes[z].Count - 1))
                    {
                        PointSectorOptRes[z][i + 1].CarZone = CountCZ;
                        LastNodesInSec.Add(PointSectorOptRes[z][i + 1]);
                    }

                }
                PointSectorOptRes[z][0].TimeDepart = TimeDepfirst;
                MakeRoute = false;

                for (int i = 0; i < PointSectorOptRes[z].Count; i++)
                    WritePlanRoute(PointSectorOptRes[z][i]);
            }
        }

        private void DeleteDouble(List<Point> ListPnt)
        {
            for (int i = 0; i < ListPnt.Count - 1; i++)
            {
                for (int j = i + 1; j < ListPnt.Count; j++)
                {
                    if (ListPnt[i].CodeTradePoint == ListPnt[j].CodeTradePoint && ListPnt[i].CodeTradePoint != "Start")
                    {
                        ListPnt.RemoveAt(j);
                        j = i;
                    }
                }
            }
        }

        private void WritePlanRoute(Point p)//записывает плановый маршрут обхода точек 
        {
            StreamWriter sw = new StreamWriter(p.RouteName + ".ltp", true, Encoding.Default);
            sw.WriteLine("p;" + p.CodeTradePoint + ";" + p.Adress + ";" + p.X_real + ";" + p.Y_real + ";" + Math.Round(p.Time * 60).ToString() + ";" + p.DistPrevTP.ToString() + ";" + p.DistanceFromFirst.ToString() +
                ";" + Math.Round(p.TimeArrive, 2).ToString() + ";" + Math.Round(p.TimeDepart, 2).ToString());
            //foreach(GMapRoute ElemRoute in p.RouteFromPrev)
            for (int i = 0; i < p.RouteFromPrev.Count; i++)
            {
                for (int j = 0; j < p.RouteFromPrev[i].Points.Count; j++)
                {
                    if (j != 0)
                    {
                        if (p.RouteFromPrev[i].Points[j].Lat != p.RouteFromPrev[i].Points[j - 1].Lat &&
                            p.RouteFromPrev[i].Points[j].Lng != p.RouteFromPrev[i].Points[j - 1].Lng)
                            sw.WriteLine(p.RouteFromPrev[i].Points[j].Lat.ToString() + ";" + p.RouteFromPrev[i].Points[j].Lng.ToString());
                    }
                    /*else
                        sw.WriteLine(p.RouteFromPrev[i].Points[j].Lat.ToString() + ";" + p.RouteFromPrev[i].Points[j].Lng.ToString());*/
                }
            }
            sw.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ReDrawPoints();
            DrawRoutes(comboBox4.Text);
        }

        private void DrawRoutes(string NameRt)
        {
            markersOverlay.Routes.Clear();
            if (NameRt == "All")
            {
                
                foreach (Point p in Reg.TradePoints)
                {
                    foreach (GMapRoute ElemRoute in p.RouteFromPrev)
                        markersOverlay.Routes.Add(ElemRoute);
                }
            }
            else
            {
                foreach (Point p in Reg.TradePoints)
                {
                    if (p.RouteName == NameRt)

                        foreach (GMapRoute ElemRoute in p.RouteFromPrev)
                        {
                            markersOverlay.Routes.Add(ElemRoute);
                        }
                }
            }
            gMapControl1.Overlays.Add(markersOverlay);
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void toolStripButton5_Click_1(object sender, EventArgs e)
        {
            label7.Text = "-";
            label11.Text = "-";
            First = true;
            CountNumDay = 0;
            clr = 0;
            GlobalCount = 0;
            gMapControl1.Overlays.Clear();
            markersOverlay.Markers.Clear();
             markersOverlay.Clear();
            polyOverlayRoute.Polygons.Clear();
            polyOverlayRoute.Clear();
            polyOverlayBordersPoly.Polygons.Clear();
            polyOverlayBordersPoly.Clear();
            polyOverlayBordersPolyChange.Polygons.Clear();
            polyOverlayBordersPolyChange.Clear();
            polyOverlayChange.Polygons.Clear();
            polyOverlayChange.Clear();
            markersOverlayStartFin.Clear();
            ChangeSector.Clear();
            if (Reg != null)
            {
                if (Reg.TradePoints.Count > 0)
                {
                    for (int i = Reg.TradePoints.Count - 1; i >= 0; i--)
                    {
                        Reg.TradePoints.RemoveAt(i);
                    }
                    Reg = null;
                    for (int i = TradePoints.Count - 1; i >= 0; i--)
                    {
                        TradePoints.RemoveAt(i);
                    }
                }
            }
            PointSector.Clear();
            PointSectorOpt.Clear();
            if (PointSectorOptRes != null)
                for (int i = 0; i < PointSectorOptRes.Length; i++)
                {
                    if (PointSectorOptRes[i] != null)
                        PointSectorOptRes[i].Clear();
                }

            LastNodesInSec.Clear();
            CountCZ = 1;
            pointsHalfWeek.Clear();
            TradePoints.Clear();
            MakeRoute = false;

            
           
            //this.Text = Constant.NameProg;
            for (int i = RoutesName.Count - 1; i >= 0; i--)
                RoutesName.RemoveAt(i);
            StartFinish = false;
            SumDist = 0;            
            label2.Text = "-";
            gMapControl1.Zoom += 0.1;
            gMapControl1.Zoom -= 0.1;
            TwoPointDist.Clear();
            CountPts = 1;
            gMapControl1.Overlays.Clear();
            markersOverlay.Markers.Clear();
            markersOverlay.Clear();
            TradePoints.Clear();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox3.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            comboBox4.Enabled = false;
            button5.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            comboBox3.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            comboBox4.Enabled = true;
            button5.Enabled = true;
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "info/manual_LTP.pdf");
        }

        private double GetDistRouteOSM(Point Pa, Point Pb, bool Table)//расчет расстояния из OSM файла
        {
            List<PointLatLng> list = new List<PointLatLng>();
            List<GMapRoute> ListRoute = new List<GMapRoute>();
            double Dist = 0;
            try
            {
                //Route route;
                var route = router.Calculate(Vehicle.Car.Shortest(), new Coordinate((float)Pa.X, (float)Pa.Y),
                     new Coordinate((float)(Pb.X + DeltaXOSM), (float)(Pb.Y + DeltaYOSM)));
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

                ErrorOSM = false;
                if (Pb.CodeTradePoint != "Start")// || !Table)
                {
                    Pb.X = Pb.X + DeltaXOSM;
                    Pb.Y = Pb.Y + DeltaYOSM;
                }
            }
            catch
            {
                ErrorOSM = true;
                return 0;                
            }

            Pb.RouteFromPrev = ListRoute;
            if (Dist < 10)
                Dist = 0;
            return Math.Round(Dist / 1000, 3);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string NameFile = "";

            switch (comboBox3.Text)
            {                
                case "Днепр":
                    gMapControl1.Position = new PointLatLng(48.489, 34.993);
                    NameFile = "Branches/Dnepr.pbf";
                    dbFileName = Constants.Path_db + "/Dnepr.db";
                    break;
               case "Запорожье":
                    gMapControl1.Position = new PointLatLng(47.79, 35.25);
                    NameFile = "Branches/Zaporozhye.pbf";
                    dbFileName = Constants.Path_db + "/Zaporozhye.db";
                    break;                
                default:
                    MessageBox.Show("Файл .pbf не найден!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
            try
            {
                
                    gMapControl1.Cursor = Cursors.WaitCursor;
                    using (var stream = File.OpenRead(NameFile))
                    {
                        routerDb.LoadOsmData(stream, Vehicle.Car);
                    }
                    routerDb.AddContracted(Vehicle.Car.Fastest());

                    comboBox3.Enabled = false;
                    gMapControl1.Cursor = Cursors.Default;
               
            }
            catch
            {
                MessageBox.Show("Отсутствует файл " + NameFile + "!", "Ошибка!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBox1.Text = "Выбор филиала";
            }
        }
    }
}
