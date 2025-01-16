using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ArabaSatisWeb.Classes
{
    public class SqlConnectionClass
    {
        public static SqlConnection connection = new SqlConnection("Data Source=DESKTOP-JQH9ME2;Initial Catalog=ArabaSatisWeb;Integrated Security=True;Encrypt=False");

        public static void CheckConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bağlantı sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public static void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
