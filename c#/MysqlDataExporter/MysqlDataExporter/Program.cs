using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.Http;

namespace MysqlDataExporter
{
    class Program
    {
       static void Main(string[] args)
        {
            MySqlCommand cmd;
            MySqlDataReader rdr = null;
            MySqlConnection conn = new MySqlConnection("Server=localhost;Database=uni;Uid=root;Pwd=root");
            conn.Open();

            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from professoren";

                rdr = cmd.ExecuteReader();

                string profs = createProfJson(rdr);
                rdr.Close();

                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from assistenten";
                rdr = cmd.ExecuteReader();
                string asst = createAsstJson(rdr);
                rdr.Close();

                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from vorlesungen";
                rdr = cmd.ExecuteReader();

                string vorl = createVorlesungJson(rdr);

                rdr.Close();

                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from studenten";
                rdr = cmd.ExecuteReader();

                string studs = createStudentJson(rdr);
                rdr.Close();


                StringBuilder dataToSend = new StringBuilder();

                dataToSend.Append("{ \"docs\": [");

               dataToSend.Append(profs);
                dataToSend.Append(",");
               dataToSend.Append(asst);
                dataToSend.Append(",");

                dataToSend.Append(vorl);
                dataToSend.Append(",");

                dataToSend.Append(studs);

                dataToSend.Append("] }");


                Task.Run(() => MainAsync(dataToSend.ToString())).GetAwaiter().GetResult();

                


            }
            catch (Exception e)
            {

                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }


        }

        static async Task MainAsync(string content)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5984");


                var stringContent = new StringContent(content.ToString(), Encoding.ASCII, "application/json");
                var result = await client.PostAsync("/uni/_bulk_docs", stringContent);
                string resultContent = await result.Content.ReadAsStringAsync();

                Console.WriteLine(resultContent);
            }
        }

        static string createProfJson(MySqlDataReader rdr )
        {
            StringBuilder sb = new StringBuilder();

            
            while (rdr.Read())
            {
                sb.Append('{');

                sb.Append("\"type\":\"Professor\",");
                string persNr = rdr.GetString(0);
                sb.Append("\"_id\":\"" + persNr + "\",");
                sb.Append("\"PersNr\":\"" + persNr + "\",");
                sb.Append("\"Name\":\"" + rdr.GetString(1) + "\",");
                sb.Append("\"Rang\":\"" + rdr.GetString(2) + "\",");
                sb.Append("\"Raum\":\"" + rdr.GetString(3) + "\"");
                sb.Append("},");

            }

            return sb.Remove(sb.Length-1,1).ToString();
        }

        static string createAsstJson(MySqlDataReader rdr)
        {
            StringBuilder sb = new StringBuilder();


            while (rdr.Read())
            {
                sb.Append('{');

                sb.Append("\"type\":\"Assistent\",");
                string persNr = rdr.GetString(0);
                sb.Append("\"_id\":\"" + persNr + "\",");

                sb.Append("\"PersNr\":\"" + persNr + "\",");
                sb.Append("\"Name\":\"" + rdr.GetString(1) + "\",");
                sb.Append("\"Fachgebiet\":\"" + rdr.GetString(2) + "\",");
                sb.Append("\"Boss\":\"" + rdr.GetString(3) + "\"");
                sb.Append("},");

            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        static string createVorlesungJson(MySqlDataReader rdr)
        {
            MySqlConnection con = new MySqlConnection("Server=localhost;Database=uni;Uid=root;Pwd=root");
            con.Open();
            MySqlDataReader reader = null;
            StringBuilder sb = new StringBuilder();

            MySqlCommand cmd = con.CreateCommand();

            while (rdr.Read())
            {
                bool hasData = false;

                sb.Append('{');

                sb.Append("\"type\":\"Vorlesung\",");

                string vorlNr = rdr.GetString(0);
                sb.Append("\"_id\":\"" + vorlNr + "\",");
                sb.Append("\"VorlNr\":\"" + vorlNr + "\",");
                sb.Append("\"Titel\":\"" + rdr.GetString(1) + "\",");
                sb.Append("\"SWS\":\"" + rdr.GetString(2) + "\",");
                sb.Append("\"gelesenVon\":\"" + rdr.GetString(3) + "\",");


                cmd.CommandText = "select * from hoeren where VorlNr = '" + vorlNr + "'";
                reader = cmd.ExecuteReader();

                sb.Append("\"besuchtVon\":[");
                
                while (reader.Read())
                {
                    hasData = true;
                    sb.Append("\"" + rdr.GetString(0) + "\",");  
                }
                if (hasData)
                    sb = sb.Remove(sb.Length - 1, 1);
                sb.Append("],");

                reader.Close();

                cmd.CommandText = "select * from voraussetzen where Nachfolger = '" + vorlNr + "'";
                reader = cmd.ExecuteReader();
                sb.Append("\"setztVoraus\":[");

                hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    sb.Append("\"" + rdr.GetString(0) + "\",");
                }

                if(hasData)
                    sb = sb.Remove(sb.Length - 1, 1);
                
                sb.Append("]");
                reader.Close();


                sb.Append("},");

            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }


        static string createStudentJson(MySqlDataReader rdr)
        {
            MySqlConnection con = new MySqlConnection("Server=localhost;Database=uni;Uid=root;Pwd=root");
            con.Open();
            MySqlDataReader reader = null;
            StringBuilder sb = new StringBuilder();

            MySqlCommand cmd = con.CreateCommand();


            while (rdr.Read())
            {
                sb.Append('{');

                sb.Append("\"type\":\"Student\",");

                string matrNr = rdr.GetString(0);
                sb.Append("\"_id\":\"" + matrNr + "\",");

                sb.Append("\"MatrNr\":\"" + matrNr + "\",");
                sb.Append("\"Name\":\"" + rdr.GetString(1) + "\",");
                sb.Append("\"Semester\":\"" + rdr.GetString(2) + "\",");


                cmd.CommandText = "select * from pruefen where MatrNr = '" + matrNr + "'";
                reader = cmd.ExecuteReader();
                sb.Append("\"PruefungenAbgelegt\":[");

                bool hasData = false;
                while (reader.Read())
                {
                    hasData = true;
                    sb.Append('{');
                    sb.Append("\"VorlNr\":\"" + matrNr + "\",");
                    sb.Append("\"geprueftVon\":\"" + rdr.GetString(1) + "\",");
                    sb.Append("\"Note\":\"" + rdr.GetString(2) + "\",");
                    sb.Append("},");

                }
                if (hasData)
                {
                    sb = sb.Remove(sb.Length - 3, 3);
                    sb.Append("}");
                }
                sb.Append("]");
                reader.Close();


                sb.Append("},");

            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
