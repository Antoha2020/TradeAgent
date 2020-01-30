using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Excel = Microsoft.Office.Interop.Excel;

namespace GmapTest
{
    public partial class PointsInRoute : Form
    {
       /* private Excel.Application excelapp;
        private Excel.Workbooks excelappworkbooks;
        private Excel.Workbook excelappworkbook;
        private Excel.Sheets excelsheets;
        private Excel.Worksheet excelworksheet;
        private Excel.Range excelcells;*/

        string Name = "";
        string dbFileName = "";

        DateTime DateT = DateTime.MaxValue;

        public PointsInRoute(string Branch, string NameRt, List<Point> Route, DateTime Dt)
        {
            InitializeComponent();

            Get_dbFileName(Branch);
            Name = NameRt;
            DateT = Dt;
            Text = Name + "  " + DateT.ToShortDateString();
            int ct = 0;
            string Code = "", Adress = "";

            foreach (Point p in Route)
            {
                if (p.Adress == "Start")
                    if (ct == 0)
                        dataGridView1.Rows.Add("", "Точка выезда", "0", "0", "0", StrDoubleToTime(p.TimeDepart));
                    else
                        dataGridView1.Rows.Add("", "Точка приезда", p.DistPrevTP.ToString(), p.DistanceFromFirst.ToString(), StrDoubleToTime(p.TimeArrive));
                else
                {
                    Code = p.CodeTradePoint;
                    Adress = p.Adress;
                    List <string> strData = DBHandler.getPoints(Code);
                    if (p.IsVisited)
                    {
                        dataGridView1.Rows.Add(++ct, Code, p.DistPrevTP.ToString(), p.DistanceFromFirst.ToString(), StrDoubleToTime(p.TimeArrive), StrDoubleToTime(p.TimeDepart),
                            p.FactTimeArrive.TimeOfDay.ToString(), p.FactTimeDepart.TimeOfDay.ToString(), Adress, strData[0], strData[1], strData[2]+" - "+strData[3]);
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        dataGridView1.Rows.Add(++ct, Code, p.DistPrevTP.ToString(), p.DistanceFromFirst.ToString(), StrDoubleToTime(p.TimeArrive), StrDoubleToTime(p.TimeDepart), "", "", Adress, strData[0], strData[1], strData[2] + " - " + strData[3]);
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        private void сохранитьВxlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* string[] NameCol = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
             string fileName = "";
             try
             {
                 excelapp = new Excel.Application();
                 excelapp.Visible = false;
                 excelapp.SheetsInNewWorkbook = 1;
                 excelapp.Workbooks.Add(Type.Missing);
                 excelappworkbooks = excelapp.Workbooks;
                 excelappworkbook = excelappworkbooks[1];
                 excelapp.DisplayAlerts = false;

                 excelsheets = excelappworkbook.Worksheets;
                 excelworksheet = (Excel.Worksheet)excelsheets.get_Item(1);

                 excelcells = excelworksheet.get_Range("A1");
                 excelcells.Font.Bold = true;
                 excelcells.Font.Italic = true;
                 excelcells.Borders.LineStyle = BorderStyle.FixedSingle;
                 excelcells.Value2 = "Информация по маршруту " + Name + " на " + DateT.ToShortDateString();

                 excelcells = excelworksheet.get_Range("A2", "L2");
                 excelcells.Font.Bold = true;
                 excelcells.Interior.Color = Color.Aqua;
                 excelcells.Borders.LineStyle = BorderStyle.FixedSingle;

                 excelcells = excelworksheet.get_Range("A2", Type.Missing);
                 excelcells.Value2 = "№ п/п";

                 excelcells = excelworksheet.get_Range("B2", Type.Missing);
                 excelcells.Value2 = "Код ТТ";

                 excelcells = excelworksheet.get_Range("C2", Type.Missing);
                 excelcells.Value2 = "Расстояние между ТТ (план)";

                 excelcells = excelworksheet.get_Range("D2", Type.Missing);
                 excelcells.Value2 = "Пробег (план)";

                 excelcells = excelworksheet.get_Range("E2", Type.Missing);
                 excelcells.Value2 = "Приезд (план)";

                 excelcells = excelworksheet.get_Range("F2", Type.Missing);
                 excelcells.Value2 = "Отъезд (план)";

                 excelcells = excelworksheet.get_Range("G2", Type.Missing);
                 excelcells.Value2 = "Приезд (факт)";

                 excelcells = excelworksheet.get_Range("H2", Type.Missing);
                 excelcells.Value2 = "Отъезд (факт)";

                 excelcells = excelworksheet.get_Range("I2", Type.Missing);
                 excelcells.Value2 = "Адрес";

                 excelcells = excelworksheet.get_Range("J2", Type.Missing);
                 excelcells.Value2 = "Контрагент";

                 excelcells = excelworksheet.get_Range("K2", Type.Missing);
                 excelcells.Value2 = "Название ТТ";

                 excelcells = excelworksheet.get_Range("L2", Type.Missing);
                 excelcells.Value2 = "Время работы";

                 for (int i = 0; i < dataGridView1.Rows.Count; i++)
                 {
                     for (int j = 0; j < dataGridView1.Columns.Count; j++)
                     {
                         excelcells = excelworksheet.get_Range(NameCol[j] + (i + 3).ToString(), Type.Missing);
                         if (dataGridView1.Rows[i].Cells[j].Value != null)
                         {
                             if (j == 2 || j == 3)
                             {
                                 double res = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
                                 excelcells.Value2 = res;
                             }
                             else
                                 excelcells.Value2 = dataGridView1.Rows[i].Cells[j].Value.ToString();

                             if (j == 6)
                             {
                                 excelcells = excelworksheet.get_Range(NameCol[0] + (i + 3).ToString(), NameCol[NameCol.Length - 1] + (i + 3).ToString());
                                 excelcells.Borders.LineStyle = BorderStyle.FixedSingle;
                                 if (dataGridView1.Rows[i].Cells[j].Value.ToString() == "")
                                     excelcells.Interior.Color = Color.LightYellow;
                                 else
                                     excelcells.Interior.Color = Color.YellowGreen;
                             }
                         }
                         else
                         {
                             excelcells.Value2 = "";
                         }
                     }
                 }

                 saveFileDialog1.Filter = "(*.xlsx)|*.xlsx|(*.xls)|*.xls|All files (*.*)|*.*";
                 saveFileDialog1.FilterIndex = 1;
                 saveFileDialog1.FileName = Name + " - " + DateT.ToShortDateString();
                 saveFileDialog1.RestoreDirectory = true;
                 if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                 {
                     fileName = saveFileDialog1.FileName;
                 }
                 else
                     return;

                 excelappworkbook.SaveAs(fileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                 saveFileDialog1.Dispose();

                 excelapp.Workbooks.Close();

                 excelapp.Quit();
                 MessageBox.Show("Отчет успешно сохранен!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
             }
             catch (Exception ex)
             {
                 MessageBox.Show("Ошибка Excel!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 excelapp.Quit();
             }
             excelapp = null;
             excelappworkbooks = null;
             excelappworkbook = null;
             excelsheets = null;
             excelworksheet = null;
             excelcells = null;*/
        }

         private void Get_dbFileName(string Br)
         {
             switch (Br)
             {                
                 case "Днепр":
                     dbFileName = Constants.Path_db + "/Dnepr.db";
                     break;               
                 case "Запорожье":
                     dbFileName = Constants.Path_db + "/Zaporozhye.db";
                     break;                
                 default:
                     return;
             }
         }

         

         private string StrDoubleToTime(double DoubleTime)
         {
             string ResStr = "";
             int Hour = 0;
             double Minute = 0;
             Hour = (int)DoubleTime;
             Minute = Math.Round((DoubleTime - Hour) * 60);
             if (Hour < 10)
                 ResStr = "0" + Hour + ":";
             else
                 ResStr = Hour + ":";

             if (Minute < 10)
                 ResStr += "0" + Minute;
             else
                 ResStr += Minute;
             return ResStr;
         }
         
         
    }
}
