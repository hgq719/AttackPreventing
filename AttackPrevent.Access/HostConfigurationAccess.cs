using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace AttackPrevent.Access
{
    public class HostConfigurationAccess
    {
        public static List<HostConfiguration> GetList()
        {
            string connString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<HostConfiguration>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT [Id], [Host], [Threshold], [Period] FROM [t_Host_Configuration]";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new HostConfiguration
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Host = Convert.ToString(reader["Host"]),
                            Threshold = Convert.ToInt32(reader["Threshold"]),
                            Period = Convert.ToInt32(reader["Period"])
                        });
                    }
                }
            }

            return result;
        }
    }
}
