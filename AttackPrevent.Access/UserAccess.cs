using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class UserAccess
    {
        public static List<dynamic> GetList()
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<dynamic> result = new List<dynamic>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT [UserName] 
                                    FROM [t_Users] ";
                SqlCommand cmd = new SqlCommand(query, conn); //comment
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = Convert.ToString(reader["UserName"]).ToLower();
                        result.Add(expando);
                    }
                }
            }

            return result;
        }
    }
}
