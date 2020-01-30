using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmapTest
{
    public class Region
    {
        public double AllDistRes = 10000000;
        public int CentralCarZone;//количество точек в центральной машино-зоне
        public List<int> BranchesRound = new List<int>();
        public List<Point> TradePoints;
        public string[] WeekRoots = new string[5] { "", "", "", "", "" };//маршруты в каждый день недели
        public bool IsFirst = true;
        public int count = 0;//временно
        public bool IsNoBranch = false;
        public double NoBrch = 0;
        public string ResString = "";
        public string EndResString = "";
        public double ResTime = 0;//нижняя граница на каждом этапе
        public double EndResTime = Constants.EndRt;//конечное, результирующее время
        public double[,] MainTable;
        public bool IsAuto = true;//true - полностью автоматический расчет; false - автоматизированный
        public string OpenFileName = "";
        public List<int> PointInSector = new List<int>();//количество машино-зон в каждом секторе
        public int SumCarZone = 0;
        public int NumVarAnal = 0;//номер варианта аналитики
        string[] Days = new string[] { "ПН", "ВТ", "СР", "ЧТ", "ПТ" };

        public Region(List<Point> TradePoints)
        {
            this.TradePoints = TradePoints;
        }

        public void SetMainTable(double[,] arr)
        {
            MainTable = new double[(int)Math.Sqrt(arr.Length), (int)Math.Sqrt(arr.Length)];
            for (int i = 0; i < (int)Math.Sqrt(arr.Length); i++)
                for (int j = 0; j < (int)Math.Sqrt(arr.Length); j++)
                {
                    MainTable[i, j] = arr[i, j];
                }
        }

        public double[,] Distances(List<Point> PointsCarZone)
        {
            double[,] DistArray = new double[PointsCarZone.Count, PointsCarZone.Count];
            for (int i = 0; i < PointsCarZone.Count; i++)
            {
                for (int j = 0; j < PointsCarZone.Count; j++)
                {
                    if (i == j)
                        DistArray[i, j] = Constants.EmptyCell;
                    else
                    {
                        //if ((TradePoints[i].WeekDay == Day1 || TradePoints[i].WeekDay == Day2) && (TradePoints[j].WeekDay == Day1 || TradePoints[j].WeekDay == Day2))
                        //if (TradePoints[i].WeekDay == Day1 && TradePoints[j].WeekDay == Day1)
                        DistArray[i, j] = Math.Round(Math.Sqrt(Math.Pow((PointsCarZone[i].X - PointsCarZone[j].X) * Constants.ParallelDist, 2) + Math.Pow((PointsCarZone[i].Y - PointsCarZone[j].Y) * Constants.MeridianDist, 2)), 3);
                        //else
                        //    DistArray[i, j] = Constant.EmptyCell;
                    }
                }
            }
            return DistArray;
        }

        public double[,] MainTask(double[,] TimeArray, string Strng)
        {
            int CountRowColumn = (int)Math.Sqrt(Convert.ToDouble(TimeArray.Length));
            double MinElem;
            double SumColRow = 0;
            double[] MinCol = new double[CountRowColumn];
            double[] MinRow = new double[CountRowColumn];

            double[] MinColFine = new double[CountRowColumn];
            double[] MinRowFine = new double[CountRowColumn];

            double[,] CopyTimeArray = new double[CountRowColumn, CountRowColumn];
            for (int j = 0; j < CountRowColumn; j++) //выполняем редукцию столбцов
            {
                MinElem = Constants.FirstMinCell;
                for (int i = 0; i < CountRowColumn; i++)
                {
                    if (TimeArray[i, j] < MinElem)
                        MinElem = TimeArray[i, j];
                }
                for (int i = 0; i < CountRowColumn; i++)
                {
                    if (TimeArray[i, j] != Constants.EmptyCell)
                        TimeArray[i, j] = Math.Round(TimeArray[i, j] - MinElem, 3);
                }
                if (MinElem != Constants.FirstMinCell)
                    MinCol[j] = MinElem;
                else
                    MinCol[j] = 0;
            }

            for (int i = 0; i < CountRowColumn; i++)//выполняем редукцию строк
            {
                MinElem = Constants.FirstMinCell;
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] < MinElem)
                        MinElem = TimeArray[i, j];
                }
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] != Constants.EmptyCell)
                        TimeArray[i, j] = Math.Round(TimeArray[i, j] - MinElem, 3);
                }
                if (MinElem != Constants.FirstMinCell)
                    MinRow[i] = MinElem;
                else
                    MinRow[i] = 0;
            }

            for (int k = 0; k < CountRowColumn; k++)//находим нижнюю границу
            {
                SumColRow += (MinCol[k] + MinRow[k]);
            }

            ResTime += SumColRow;

            if (ResTime >= EndResTime && !IsNoBranch)
                return TimeArray;

            if (IsNoBranch)
            {
                ResTime = NoBrch;
            }
            Dictionary<string, double> Fines = new Dictionary<string, double>();
            //double MinFines = Constant.FirstMinCell;

            for (int i = 0; i < CountRowColumn; i++) //определяем альтернативы для нулей строк
            {
                int CountZero = 0;
                MinRowFine[i] = Constants.FirstMinCell;
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] == 0)
                    {
                        CountZero++;
                        if (CountZero == 1)
                            continue;
                        else
                        {
                            MinRowFine[i] = 0;
                            break;
                        }
                    }
                    if (MinRowFine[i] > TimeArray[i, j])
                        MinRowFine[i] = TimeArray[i, j];
                }
            }

            for (int j = 0; j < CountRowColumn; j++) //определяем альтернативы для нулей столбцов
            {
                int CountZero = 0;
                MinColFine[j] = Constants.FirstMinCell;
                for (int i = 0; i < CountRowColumn; i++)
                {
                    if (TimeArray[i, j] == 0)
                    {
                        CountZero++;
                        if (CountZero == 1)
                            continue;
                        else
                        {
                            MinColFine[j] = 0;
                            break;
                        }
                    }
                    if (MinColFine[j] > TimeArray[i, j])
                        MinColFine[j] = TimeArray[i, j];
                }
            }

            for (int i = 0; i < CountRowColumn; i++) //определяем штрафы для нулевых ячеек
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] == 0)
                    {
                        Fines.Add((i + 1).ToString() + "-" + (j + 1).ToString(), MinColFine[j] + MinRowFine[i]);
                    }
                }

            double MaxFine = 0;
            foreach (KeyValuePair<string, double> Val in Fines)//определяем максимальный штраф
            {
                if (Val.Value > MaxFine)
                    MaxFine = Val.Value;
            }

            string FineRowCol = "";
            foreach (KeyValuePair<string, double> Val in Fines)//определяем столбец и строку, которые вычеркиваются
            {
                if (Val.Value == MaxFine)
                    FineRowCol = Val.Key;
            }

            double NoBranch = ResTime + MaxFine;
            string[] StrArr = FineRowCol.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            int R = Convert.ToInt32(StrArr[0]) - 1;
            int C = Convert.ToInt32(StrArr[1]) - 1;


            if (IsNoBranch)
            {
                ResString = Strng;
                IsNoBranch = false;
            }


            for (int i = 0; i < CountRowColumn; i++)
                for (int j = 0; j < CountRowColumn; j++)
                {
                    CopyTimeArray[i, j] = TimeArray[i, j];
                }
            string StrNow = ResString;
            ResString += FineRowCol + ";";
            Looping(R, C);

            int ct1 = 0; //определяем сколько ячеек не равны 1000000
            for (int i = 0; i < CountRowColumn; i++)
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] != Constants.EmptyCell)
                        ct1++;
                }

            for (int i = 0; i < CountRowColumn; i++)
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (i == R || j == C)
                        TimeArray[i, j] = Constants.EmptyCell;
                }

            for (int z = 0; z < BranchesRound.Count; z = z + 2)//определение ветви, которая входит в решение
            {

                TimeArray[BranchesRound[z + 1], BranchesRound[z]] = Constants.EmptyCell;
                if (ct1 == 2)
                {
                    ResString += (BranchesRound[z + 1] + 1).ToString() + "-" + (BranchesRound[z] + 1).ToString();
                    Looping(BranchesRound[z + 1], BranchesRound[z]);

                }
            }

            int ct = 0; //определяем сколько ячеек не равны 1000000
            for (int i = 0; i < CountRowColumn; i++)
                for (int j = 0; j < CountRowColumn; j++)
                {
                    if (TimeArray[i, j] != Constants.EmptyCell)
                        ct++;
                }

            if (ct != 0)
            {
                MainTask(TimeArray, "");
                if (IsAuto)
                {
                    if (NoBranch < EndResTime)
                    {
                        CopyTimeArray[Convert.ToInt32(StrArr[0]) - 1, Convert.ToInt32(StrArr[1]) - 1] = Constants.EmptyCell;
                        IsNoBranch = true;
                        NoBrch = NoBranch;
                        string[] S = StrNow.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = BranchesRound.Count - 1; i >= 0; i--)
                            BranchesRound.RemoveAt(i);
                        if (S.Length == 0)
                            IsFirst = true;
                        else
                        {
                            for (int i = 0; i < S.Length; i++)
                            {
                                string[] S1 = S[i].Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                Looping(Convert.ToInt32(S1[0]) - 1, Convert.ToInt32(S1[1]) - 1);
                            }
                        }
                        MainTask(CopyTimeArray, StrNow);
                    }
                }

                return TimeArray;
            }
            else
            {
                EndResTime = ResTime;
                EndResString = ResString;
                return TimeArray;
            }
        }

        private void Looping(int R, int C)//не позволяет зацикливать маршрут до обхода всех вершин
        {
            if (IsFirst)
            {
                BranchesRound.Add(R);
                BranchesRound.Add(C);
                IsFirst = false;
            }
            else
            {
                bool enter = false;
                for (int i = 0; i < BranchesRound.Count; i = i + 2)
                {
                    if (R == BranchesRound[i + 1])
                    {
                        BranchesRound[i + 1] = C;//добавляем вершину справа
                        enter = true;
                        break;
                    }

                    if (C == BranchesRound[i])
                    {
                        BranchesRound[i] = R;//добавляем вершину слева
                        enter = true;
                        break;
                    }
                }

                if (!enter)
                {
                    BranchesRound.Add(R);
                    BranchesRound.Add(C);
                }
            }
            bool Exit = false;
            for (int m = 0; m < BranchesRound.Count; m += 2)
            {
                for (int n = 0; n < BranchesRound.Count; n += 2)
                {
                    if (BranchesRound[m] == BranchesRound[n + 1])
                    {
                        BranchesRound[m] = BranchesRound[n];
                        BranchesRound.RemoveAt(n);
                        BranchesRound.RemoveAt(n);
                        Exit = true;
                        break;
                    }
                }
                if (Exit)
                    break;
            }
        }

        public int InsidePolygon(List<double> xp, List<double> yp)
        {
            int k = 0;
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
                    TrPoint.InPoly = true;
                    k++;
                }
            }
            //SetWeekDaysTwice();
            return k;
        }

        public void TwoVisit(List<string> StrBr)
        {
            List<Point> Lac = new List<Point>();
            foreach (Point p in TradePoints)
                p.NumVisit = 1;
            for (int k = 0; k < StrBr.Count; k++)
            {
                for (int i = Lac.Count - 1; i >= 0; i--)
                    Lac.RemoveAt(i);
                foreach (Point p in TradePoints)
                {
                    if (p.Brand == StrBr[k])
                        Lac.Add(p);
                }

                for (int i = 0; i < Lac.Count; i++)
                {
                    for (int j = 0; j < Lac.Count; j++)
                    {
                        if (Lac[i].CodeTradePoint == Lac[j].CodeTradePoint && i != j)
                        {
                            Lac[i].NumVisit++;
                        }
                    }
                }
            }
        }

        public double FirstEndTime = 0;
        public string OptimalRouteMain(double[,] Arr)
        {
            string StrRes = "";
            string StrResAll = "";
            int NumRowCol = (int)(Math.Sqrt(Arr.Length));
            List<double>[] DistanceInRow = new List<double>[NumRowCol];
            int NumMaxSumRow = 0;//строка с максимальной суммой значений
            double MinSum = 0, MaxSum = 10000000;
            int NumRow = NumMaxSumRow;
            int First = NumMaxSumRow;
            double AllDist = 0;
            AllDistRes = 10000000;
            for (int i = 0; i < NumRowCol; i++)
                DistanceInRow[i] = new List<double>();

            for (int m = 0; m < NumRowCol; m++)
            {
                for (int i = 0; i < NumRowCol; i++)
                {
                    for (int j = 0; j < NumRowCol; j++)
                        DistanceInRow[i].Add(Arr[i, j]);
                    if (DistanceInRow[i].Sum() > MinSum && DistanceInRow[i].Sum() < MaxSum)
                    {
                        MinSum = DistanceInRow[i].Sum();
                        //NumMaxSumRow = i;
                        NumRow = First = i;
                    }
                }
                //NumRow = NumMaxSumRow;
                MaxSum = MinSum;
                for (int i = 0; i < NumRowCol; i++)
                {
                    int NumCol = DistanceInRow[NumRow].IndexOf(DistanceInRow[NumRow].Min());
                    AllDist += DistanceInRow[NumRow].Min();
                    for (int k = 0; k < DistanceInRow[NumRow].Count; k++)
                    {
                        DistanceInRow[NumRow][k] = Constants.EmptyCell;
                        DistanceInRow[k][NumCol] = Constants.EmptyCell;
                    }

                    if (i != NumRowCol - 1)
                    {
                        /*if (NumRow == 0)
                            FirstEndTime += DistanceInRow[NumRow][NumCol];
                        if(NumCol==0)
                            FirstEndTime += DistanceInRow[NumRow][NumCol];*/
                        StrRes += (NumRow + 1).ToString() + "-" + (NumCol + 1).ToString() + ";";
                        if (i != NumRowCol - 2)
                            DistanceInRow[NumCol][First] = Constants.EmptyCell;
                    }
                    else
                    {
                        //AllDist += DistanceInRow[NumRow].Min();
                        StrRes += (NumRow + 1).ToString() + "-" + (First + 1).ToString();
                        if (AllDist < AllDistRes)
                        {
                            AllDistRes = AllDist;
                            StrResAll = StrRes;
                        }

                        for (int a = 0; a < NumRowCol; a++)
                            for (int b = NumRowCol - 1; b >= 0; b--)
                                DistanceInRow[a].RemoveAt(b);
                        AllDist = 0;
                        StrRes = "";
                        MinSum = 0;
                    }
                    NumRow = NumCol;
                }
            }

            return StrResAll;
        }

        public void ResolveFirstEnd(string Str)
        {
            string[] StrArr = Str.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < StrArr.Length; i++)
            {
                string[] StrArr1 = StrArr[i].Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (StrArr1[0] != "1" && StrArr1[1] != "1")
                {
                    double Dst = MainTable[Convert.ToInt32(StrArr1[0]) - 1, Convert.ToInt32(StrArr1[1]) - 1];
                    double Speed = 0;
                    if (Dst < 0.3)
                        Speed = Constants.SpeedFoot;
                    else
                    {
                        if (Dst < 5)
                            Speed = Constants.SpeedCar;
                        else
                            Speed = Constants.SpeedCarHW;
                    }
                    FirstEndTime += Math.Round(Dst / Speed, 2) + TradePoints[Convert.ToInt32(StrArr1[1]) - 1].Time;
                }
            }
        }
    }
}
