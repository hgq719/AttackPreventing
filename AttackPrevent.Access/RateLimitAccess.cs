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
        public static List<RateLimitEntity> GetRateLimits(string zoneID, DateTime? startTime, DateTime? endTime, string url)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<RateLimitEntity> result = new List<RateLimitEntity>();
            StringBuilder query = new StringBuilder("select Period, Threshold, Url, OrderNo, EnlargementFactor, RateLimitTriggerIpCount, Id from t_RateLimiting_Rules where ZoneId=@zoneID");
            if (startTime.HasValue)
            {
                query.Append(" and LogTime >= @startTime ");
            }

            if (endTime.HasValue)
            {
                query.Append(" and LogTime <= @endTime ");
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                query.Append(" and Url = @url ");
            }

            query.Append(" ORDER BY OrderNo");

            using (SqlConnection conn = new SqlConnection(cons))
            {
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneID);
                if (startTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@startTime", startTime.Value);
                }
                if (endTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@endTime", endTime.Value);
                }
                if (!string.IsNullOrWhiteSpace(url))
                {
                    cmd.Parameters.AddWithValue("@url", url);
                }
                
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
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

        public static void Add(RateLimitEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"INSERT INTO dbo.t_RateLimiting_Rules
        ( ZoneId ,
          OrderNo ,
          Url ,
          Threshold ,
          Period ,
          Action ,
          EnlargementFactor ,
          LatestTriggerTime ,
          RateLimitTriggerIpCount ,
          RateLimitTriggerTime ,
          Remark ,
          CreatedBy ,
          CreatedTime
        )
VALUES  ( @zoneID , -- ZoneId - nvarchar(512)
          (SELECT COUNT(1) FROM dbo.t_RateLimiting_Rules) + 1 , -- OrderNo - int
          @url , -- Url - nvarchar(512)
          @threshold , -- Threshold - int
          @period , -- Period - int
          N'challenge' , -- Action - nvarchar(256)
          @enlargement , -- EnlargementFactor - int
          GETDATE() , -- LatestTriggerTime - datetime
          @triggerIpCount , -- RateLimitTriggerIpCount - int
          @triggerTime , -- RateLimitTriggerTime - int
          N'' , -- Remark - nvarchar(1024)
          @user , -- CreatedBy - 
          GETDATE()  -- CreatedTime - datetime
        )";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                cmd.Parameters.AddWithValue("@url", item.Url);
                cmd.Parameters.AddWithValue("@enlargement", item.EnlargementFactor);
                cmd.Parameters.AddWithValue("@triggerIpCount", item.RateLimitTriggerIpCount);
                cmd.Parameters.AddWithValue("@triggerTime", item.RateLimitTriggerTime);
                cmd.Parameters.AddWithValue("@user", item.CreatedBy);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(RateLimitEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"UPDATE  dbo.t_RateLimiting_Rules
SET     ZoneId = @zoneID ,
        Url = @url ,
        Threshold = @threshold ,
        Period = @period ,
        EnlargementFactor = @enlargement ,
        RateLimitTriggerIpCount = @triggerIpCount ,
        RateLimitTriggerTime = @triggerTime ,
        CreatedBy = @user
WHERE   Id = @id;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                cmd.Parameters.AddWithValue("@url", item.Url);
                cmd.Parameters.AddWithValue("@enlargement", item.EnlargementFactor);
                cmd.Parameters.AddWithValue("@triggerIpCount", item.RateLimitTriggerIpCount);
                cmd.Parameters.AddWithValue("@triggerTime", item.RateLimitTriggerTime);
                cmd.Parameters.AddWithValue("@user", item.CreatedBy);
                cmd.Parameters.AddWithValue("@id", item.TableID);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void EditOrder(int fromID, int fromOrder, int toID, int toOrder)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"UPDATE dbo.t_RateLimiting_Rules SET OrderNo=@fromOrder WHERE Id=@fromID;
                                 UPDATE dbo.t_RateLimiting_Rules SET OrderNo=@toOrder WHERE Id=@toID;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fromOrder", fromOrder);
                cmd.Parameters.AddWithValue("@fromID", fromID);
                cmd.Parameters.AddWithValue("@toOrder", toOrder);
                cmd.Parameters.AddWithValue("@toID", toID);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id, int order)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"DELETE dbo.t_RateLimiting_Rules WHERE Id=@id;
                                 UPDATE dbo.t_RateLimiting_Rules SET OrderNo= OrderNo - 1 WHERE OrderNo > @order;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@order", order);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static RateLimitEntity GetRateLimitByOrderNo(int order)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            RateLimitEntity item = new RateLimitEntity();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "select Period, Threshold, Url, OrderNo, EnlargementFactor, RateLimitTriggerIpCount, Id, ZoneId, RateLimitTriggerTime from t_RateLimiting_Rules where OrderNo=@order";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@order", order);
                conn.Open();
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        
                        item.Period = reader.GetInt32(0);
                        item.Threshold = reader.GetInt32(1);
                        item.Url = reader.GetString(2);
                        item.OrderNo = reader.GetInt32(3);
                        item.EnlargementFactor = reader.GetInt32(4);
                        item.RateLimitTriggerIpCount = reader.GetInt32(5);
                        item.TableID = reader.GetInt32(6);
                        item.ZoneId = reader.GetString(7);
                        item.RateLimitTriggerTime = reader.GetInt32(8);
                    }
                }
            }

            return item;
        }

        public static RateLimitEntity GetRateLimitByID(int id)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            RateLimitEntity item = new RateLimitEntity();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "select Period, Threshold, Url, OrderNo, EnlargementFactor, RateLimitTriggerIpCount, Id, ZoneId, RateLimitTriggerTime from t_RateLimiting_Rules where Id=@id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        item.Period = reader.GetInt32(0);
                        item.Threshold = reader.GetInt32(1);
                        item.Url = reader.GetString(2);
                        item.OrderNo = reader.GetInt32(3);
                        item.EnlargementFactor = reader.GetInt32(4);
                        item.RateLimitTriggerIpCount = reader.GetInt32(5);
                        item.TableID = reader.GetInt32(6);
                        item.ZoneId = reader.GetString(7);
                        item.RateLimitTriggerTime = reader.GetInt32(8);
                    }
                }
            }

            return item;
        }

        public static int GetRateLimitMaxOrder()
        {
            int count = 0;
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "SELECT MAX(OrderNo) FROM dbo.t_RateLimiting_Rules";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                count = (int)cmd.ExecuteScalar();
            }

            return count;
        }

    }
}
