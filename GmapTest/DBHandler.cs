using GMap.NET;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class DBHandler
    {
        public static MySqlConnection conn;
        public static void DBConnection()
        {
            String connString = "Server=" + Constants.HOST + ";Database=" + Constants.DATABASE
                + ";port=" + Constants.PORT + ";User Id=" + Constants.USERNAME + ";password=" + Constants.PASSWORD;
            conn = new MySqlConnection(connString);            
        }

        public string getAuth(string Login, string pwd) //проверка логина и пароля в базе данных
        {
            DBConnection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader["login"].Equals(Login) && reader["password"].Equals(pwd))
                {
                    reader.Close();
                    conn.Close();
                    return Login;
                }
            }

            reader.Close();
            conn.Close();
            return null;
        }

        public static List<Route> GetListRoutes()
        {
            List<Route> listRoutes = new List<Route>();
            DBConnection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM routes";
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listRoutes.Add(new Route(reader["codeGPS"].ToString(), reader["name"].ToString(), reader["latBeg"].ToString(),
                    reader["lonBeg"].ToString(), reader["team"].ToString(), reader["branch"].ToString()));                
            }

            reader.Close();
            conn.Close();
            return listRoutes;
        }


        public static List<string> getPoints(string Code)
        {
            bool enter = false;
            List<string> listData = new List<string>();
            DBConnection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name_contragent FROM tradePoints WHERE code='" + Code.Trim() + "'";
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["name_contragent"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT name_point FROM tradePoints WHERE code='" + Code.Trim() + "'";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["name_point"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT work_time_beg FROM tradePoints WHERE code='" + Code.Trim() + "'"; 
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["work_time_beg"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT work_time_end FROM tradePoints WHERE code='" + Code.Trim() + "'";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["work_time_end"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();
            conn.Close();

            return listData;
        }
        
    }
}
