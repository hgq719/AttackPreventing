using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AttackPrevent.Access
{
    public class GlobalConfigurationAccess
    {
        public static List<GlobalConfiguration> GetList()
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<GlobalConfiguration>();
            using (var conn = new SqlConnection(cons))
            {
                var query = @"SELECT [EmailAddForWhiteList],
                                        [CancelBanIPTime],
                                        [ValidateCode],
                                        [GlobalSample],
                                        [GlobalTimeSpan],
                                        [CancelAttackTime]
                                        FROM [t_Global_Configuration] ";
                var cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new GlobalConfiguration
                        {
                            EmailAddForWhiteList = Convert.ToString(reader["EmailAddForWhiteList"]),
                            CancelBanIPTime = Convert.ToInt32(reader["CancelBanIPTime"]),
                            ValidateCode = Convert.ToString(reader["ValidateCode"]),
                            GlobalSample = Convert.ToDouble(reader["GlobalSample"]),
                            GlobalTimeSpan = Convert.ToInt32(reader["GlobalTimeSpan"]),
                            CancelAttackTime = Convert.ToInt32(reader["CancelAttackTime"])
                        });
                    }
                }
            }

            return result;
        }
    }
}
