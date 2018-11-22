using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class AuditLogAccess
    {
        public static List<AuditLogEntity> GetList(string zoneID, DateTime startTime, DateTime endTime, string logType, string detail)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<AuditLogEntity> result = new List<AuditLogEntity>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "select LogLevel, LogTime, LogOperator, Detail from t_Logs where ZoneId=@zoneID and LogTime >= @startTime and LogTime <= @endTime and LogLevel in (@logType) and Detail like'%@detail%'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneID);
                cmd.Parameters.AddWithValue("@startTime", startTime);
                cmd.Parameters.AddWithValue("@endTime", endTime);
                cmd.Parameters.AddWithValue("@logType", logType);
                cmd.Parameters.AddWithValue("@detail", detail);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 0;
                    while (reader.Read())
                    {
                        AuditLogEntity item = new AuditLogEntity();
                        item.ID = index++;
                        item.LogType = reader.GetString(0);
                        item.LogTime = reader.GetDateTime(1);
                        item.LogOperator = reader.GetString(2);
                        item.Detail = reader.GetString(3);
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
