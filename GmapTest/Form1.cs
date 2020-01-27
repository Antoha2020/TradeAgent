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
        private bool StartFinish = false;
        double FirstLat = 0, FirstLng = 0;
        private List<PointLatLng> TwoPointDist = new List<PointLatLng>();
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

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            StartFinish = false;
            SumDist = 0;
            ClearMap();
            //TradePoints.Clear();
            label2.Text = "-";
            gMapControl1.Zoom += 0.1;
            gMapControl1.Zoom -= 0.1;
            TwoPointDist.Clear();
            CountPts = 1;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(48.489, 34.993);
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;
            gMapControl1.DragButton = MouseButtons.Left;
            Logger.Log.Info("Form1 загружена");
        }

        
        private void gMapControl1_OnMapZoomChanged()
        {
            trackBar1.Value = (int)gMapControl1.Zoom;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)// тестовая кнопка, убрать
        {
            
        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            //определение расстояния по прямой

            List<double> xp = new List<double>();
            List<double> yp = new List<double>();
            /*if (e.Button == MouseButtons.Right)
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


             if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
             {
                 //textBox1.Text = "Записать в файл";
                 //textBox1.Enabled = false;

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
                 label7.Text = CountInDisrt.ToString();
                 CountInDisrt = 0;
                 for (int i = pointsHalfWeek.Count - 1; i >= 0; i--)
                     pointsHalfWeek.RemoveAt(i);

                 //textBox1.Enabled = true;
                 //textBox1.Text = "";
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
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

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
                        if(plan == "П" && fact != "Ф")                        
                            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name, plan+"/-",
                            ResParam[0], ResParam[1], "", "", "", "");
                        if(plan != "П" && fact == "Ф")
                            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, rt.Name, "-/" +fact,
                            ResParam[0], ResParam[1], ResParam[2], "", ResParam[4], "");

                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
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

        private void ClearMap()//очистить карту
        {
            gMapControl1.Overlays.Clear();
            markersOverlay.Markers.Clear();
            markersOverlay.Clear();            
            TradePoints.Clear();            
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
    }
}
