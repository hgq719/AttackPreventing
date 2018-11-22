using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                string query = "select [UserName] from [t_Users] ";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new { Name= reader.GetString(0).ToLower()});
                    }
                }
            }

            return result;
        }
    }
}
