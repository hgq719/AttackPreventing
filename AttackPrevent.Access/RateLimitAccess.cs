using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class RateLimitAccess
    {
        public static List<RateLimitEntity> GetRateLimits(string zoneID, DateTime startTime, DateTime endTime, string url)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<RateLimitEntity> result = new List<RateLimitEntity>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "select Period, Threshold, Url, OrderNo, EnlargementFactor, RateLimitTriggerIpCount, Id from t_RateLimiting_Rules where ZoneId=@zoneID and LogTime >= @startTime and LogTime <= @endTime and Url = @url";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneID);
                cmd.Parameters.AddWithValue("@startTime", startTime);
                cmd.Parameters.AddWithValue("@endTime", endTime);
                cmd.Parameters.AddWithValue("@url", url);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 0;
                    while (reader.Read())
                    {
                        RateLimitEntity item = new RateLimitEntity();
                        item.ID = index++;
                        item.Period = reader.GetInt32(0);
                        item.Threshold = reader.GetInt32(1);
                        item.Url = reader.GetString(2);
                        item.OrderNo = reader.GetInt32(3);
                        item.EnlargementFactor = reader.GetInt32(4);
                        item.RateLimitTriggerIpCount = reader.GetInt32(5);
                        item.TableID = reader.GetInt32(6);
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
