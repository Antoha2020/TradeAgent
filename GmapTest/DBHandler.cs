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
        public MySqlConnection conn;
        public void DBConnection()
        {
            String connString = "Server=" + Constants.HOST + ";Database=" + Constants.DATABASE
                + ";port=" + Constants.PORT + ";User Id=" + Constants.USERNAME + ";password=" + Constants.PASSWORD;
            conn = new MySqlConnection(connString);            
        }

        public string getAuth(string Login, string pwd)
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


        public Dictionary<long,PointLatLng> getDataTEST(string table)
        {
            try
            {
                //String result = "";
                Dictionary<long, PointLatLng> fact = new Dictionary<long, PointLatLng>();
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    fact.Add(Convert.ToInt64(reader[1]), new PointLatLng(Convert.ToDouble(reader[2]), Convert.ToDouble(reader[3])));
                    //result = reader[2].ToString() + "  " + reader[2].ToString() + "  " + reader[3].ToString();
                }
                conn.Close();
                return fact;
            }
            catch { return null; }
        }

        public List<PointLatLng> getDataListTEST(string table)
        {
            try
            {

                //String result = "";
                List<PointLatLng> fact = new List<PointLatLng>();
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + table;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    fact.Add(new PointLatLng(Convert.ToDouble(reader[2].ToString().Replace(".",",")), Convert.ToDouble(reader[3].ToString().Replace(".",","))));
                    //result = reader[2].ToString() + "  " + reader[2].ToString() + "  " + reader[3].ToString();
                }
                conn.Close();
                return fact;
            }
            catch { return null; }
        }

        public List<PointLatLng> getPointsTEST(List<int> idPoints)
        {
            List<PointLatLng> result = new List<PointLatLng>();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            foreach (int id in idPoints)
            {
                cmd.CommandText = "SELECT lat, lon FROM tradePoints where id=" + id.ToString();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new PointLatLng(Convert.ToDouble(reader[0].ToString()), Convert.ToDouble(reader[1].ToString())));
                }
                reader.Close();
            }
            conn.Close();
            return result;
        }

        public void insertFactTEST(List<double> lat, List<double> lon, List<long> time)
        {
            List<PointLatLng> result = new List<PointLatLng>();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            for(int i=0;i<lat.Count;i++)
            {
                cmd.CommandText = "INSERT INTO route2222_17122019(timestamp,latitude,longitude)" +
                    "values(" + time[i] + ",'" + lat[i] + "','" + lon[i] + "')";
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }

        public void insertSerializeTEST(string imei, string date, string factRoute)
        {
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO routes(imei,date,factRoute)" + "values('" + imei + "','" + date + "','" + factRoute + "')";
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public string getSerialTEST()
        {
            try
            {
                String result = "";
                List<PointLatLng> fact = new List<PointLatLng>();
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT factRoute FROM routes where imei='1111'";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // fact.Add(new PointLatLng(Convert.ToDouble(reader[2]), Convert.ToDouble(reader[3])));
                    result = reader[0].ToString();// + "  " + reader[2].ToString() + "  " + reader[3].ToString();
                }
                conn.Close();
                return result;
            }
            catch { return null; }
        }

        public List<TransObject> getTransObjects()
        {
            List<TransObject> result = new List<TransObject>();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            
                cmd.CommandText = "SELECT * FROM objects";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new TransObject(reader["garageNumber"].ToString(), reader["model"].ToString(),reader["imei"].ToString()));
                }
                reader.Close();
            
            conn.Close();
            return result;
        }
    }
}
