using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class GlobalConfigurationAccess
    {
        public static List<GlobalConfiguration> GetList()
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<GlobalConfiguration> result = new List<GlobalConfiguration>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "SELECT [EmailAddForWhiteList],[CancelBanIPTime],[UrlCheckForAlert],[ValidateCode] FROM [t_Global_Configuration] ";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new GlobalConfiguration
                        {
                            EmailAddForWhiteList = reader.GetString(0),
                            CancelBanIPTime = reader.GetInt32(1),
                            UrlCheckForAlert = reader.GetString(2),
                            ValidateCode = reader.GetString(3),
                        });
                    }
                }
            }

            return result;
        }
    }
}
