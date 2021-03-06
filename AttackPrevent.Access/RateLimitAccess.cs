﻿using AttackPrevent.Model;
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
        public static List<RateLimitEntity> GetRateLimits(string zoneId, DateTime? startTime, DateTime? endTime, string url)
        {
            var cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<RateLimitEntity>();
            var query = new StringBuilder(@"SELECT Period, 
                                                             Threshold, 
                                                             Url, 
                                                             OrderNo, 
                                                             EnlargementFactor, 
                                                             RateLimitTriggerIpCount, 
                                                             RateLimitTriggerTime,
                                                             LatestTriggerTime,
                                                             ZoneId,
                                                             Action,
                                                             CreatedBy,
                                                             IfBanIp,
                                                             IfOpenRateLimitRule,
                                                             IfTesting,
                                                             Id FROM t_RateLimiting_Rules where ZoneId=@zoneID");
            if (startTime.HasValue)
            {
                query.Append(" AND CreatedTime >= @startTime ");
            }

            if (endTime.HasValue)
            {
                query.Append(" AND CreatedTime <= @endTime ");
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                query.Append(" AND Url LIKE'%'+@url+'%' ");
            }

            query.Append(" ORDER BY OrderNo");

            using (var conn = new SqlConnection(cons))
            {
                var cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneId);
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

                using (var reader = cmd.ExecuteReader())
                {
                    var index = 1;
                    while (reader.Read())
                    {
                        result.Add(new RateLimitEntity
                        {
                            ID = index++,
                            Period = Convert.ToInt32(reader["Period"]),
                            Threshold = Convert.ToInt32(reader["Threshold"]),
                            Url = Convert.ToString(reader["Url"]).Replace("https://", "").Replace("http://", "").Trim(),
                            OrderNo = Convert.ToInt32(reader["OrderNo"]),
                            EnlargementFactor =float.Parse( reader["EnlargementFactor"] == DBNull.Value ? "0" : reader["EnlargementFactor"].ToString()),
                            RateLimitTriggerIpCount = Convert.ToInt32(reader["RateLimitTriggerIpCount"]),
                            RateLimitTriggerTime = Convert.ToInt32(reader["RateLimitTriggerTime"]),
                            LatestTriggerTime = Convert.ToDateTime(reader["LatestTriggerTime"]),
                            TableID = Convert.ToInt32(reader["Id"]),
                            ZoneId = reader["ZoneId"].ToString(),
                            Action = reader["Action"].ToString(),
                            CreatedBy = reader["CreatedBy"].ToString(),
                            IfBanIp = Convert.ToInt32(reader["IfBanIp"]) > 0,
                            IfOpenRateLimitRule = Convert.ToInt32(reader["IfOpenRateLimitRule"]) > 0,
                            IfTesting = Convert.ToInt32(reader["IfTesting"]) > 0
                        });
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
          CreatedTime,
          IfBanIp,
          IfOpenRateLimitRule,
          IfTesting
        )
VALUES  ( @zoneID , -- ZoneId - nvarchar(512)
          @order , -- OrderNo - int
          @url , -- Url - nvarchar(512)
          @threshold , -- Threshold - int
          @period , -- Period - int
          N'challenge' , -- Action - nvarchar(256)
          @enlargement , -- EnlargementFactor - int
          GETUTCDATE() , -- LatestTriggerTime - datetime
          @triggerIpCount , -- RateLimitTriggerIpCount - int
          @triggerTime , -- RateLimitTriggerTime - int
          N'' , -- Remark - nvarchar(1024)
          @user , -- CreatedBy - 
          GETUTCDATE(),  -- CreatedTime - datetime
          @ifBanIp , -- IfBanIp - 
          @ifOpenRateLimitRule , -- IfOpenRateLimitRule - 
          @ifTesting  -- IfTesting - 
        )";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@order", item.OrderNo);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                cmd.Parameters.AddWithValue("@url", item.Url);
                cmd.Parameters.AddWithValue("@enlargement", item.EnlargementFactor);
                cmd.Parameters.AddWithValue("@triggerIpCount", item.RateLimitTriggerIpCount);
                cmd.Parameters.AddWithValue("@triggerTime", item.RateLimitTriggerTime);
                cmd.Parameters.AddWithValue("@user", item.CreatedBy);
                cmd.Parameters.AddWithValue("@ifBanIp", item.IfBanIp);
                cmd.Parameters.AddWithValue("@ifOpenRateLimitRule", item.IfOpenRateLimitRule);
                cmd.Parameters.AddWithValue("@ifTesting", item.IfTesting);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(RateLimitEntity item)
        {
            var cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (var conn = new SqlConnection(cons))
            {
                const string query = @"UPDATE  dbo.t_RateLimiting_Rules
                                    SET     ZoneId = @zoneID ,
                                            Url = @url ,
                                            Threshold = @threshold ,
                                            Period = @period ,
                                            EnlargementFactor = @enlargement ,
                                            RateLimitTriggerIpCount = @triggerIpCount ,
                                            RateLimitTriggerTime = @triggerTime ,
                                            CreatedBy = @user,
                                            IfBanIp = @ifBanIp,
                                            IfOpenRateLimitRule = @ifOpenRateLimitRule,
                                            IfTesting = @ifTesting
                                    WHERE   Id = @id;";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                cmd.Parameters.AddWithValue("@url", item.Url);
                cmd.Parameters.AddWithValue("@enlargement", item.EnlargementFactor);
                cmd.Parameters.AddWithValue("@triggerIpCount", item.RateLimitTriggerIpCount);
                cmd.Parameters.AddWithValue("@triggerTime", item.RateLimitTriggerTime);
                cmd.Parameters.AddWithValue("@user", item.CreatedBy);
                cmd.Parameters.AddWithValue("@id", item.TableID);
                cmd.Parameters.AddWithValue("@ifBanIp", item.IfBanIp);
                cmd.Parameters.AddWithValue("@ifOpenRateLimitRule", item.IfOpenRateLimitRule);
                cmd.Parameters.AddWithValue("@ifTesting", item.IfTesting);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void TriggerRateLimit(RateLimitEntity rateLimit)
        {
            var cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (var conn = new SqlConnection(cons))
            {
                const string query = @"UPDATE dbo.t_RateLimiting_Rules
                                 SET LatestTriggerTime = GETUTCDATE()  
                                 WHERE Url = @Url 
                                   AND Threshold = @Threshold
                                   AND Period = @Period
                                   AND ZoneId = @ZoneId";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Threshold", rateLimit.Threshold);
                cmd.Parameters.AddWithValue("@Period", rateLimit.Period);
                cmd.Parameters.AddWithValue("@Url", rateLimit.Url);
                cmd.Parameters.AddWithValue("@ZoneId", rateLimit.ZoneId);
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

        public static void Delete(int id, int order, string zoneId)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"DELETE dbo.t_RateLimiting_Rules WHERE Id=@id;
                                 UPDATE dbo.t_RateLimiting_Rules SET OrderNo= OrderNo - 1 WHERE OrderNo > @order AND ZoneId = @zoneId;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@order", order);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static RateLimitEntity GetRateLimitByOrderNo(int order, string zoneId)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            RateLimitEntity item = new RateLimitEntity();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT Period, 
                                        Threshold, 
                                        Url, 
                                        OrderNo, 
                                        EnlargementFactor, 
                                        RateLimitTriggerIpCount, 
                                        Id, 
                                        ZoneId, 
                                        IfBanIp,
                                        IfOpenRateLimitRule,
                                        IfTesting,
                                        RateLimitTriggerTime FROM t_RateLimiting_Rules where OrderNo=@order and ZoneId=@zoneId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@order", order);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                conn.Open();
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        
                        item.Period = Convert.ToInt32(reader["Period"]);
                        item.Threshold = Convert.ToInt32(reader["Threshold"]);
                        item.Url = Convert.ToString(reader["Url"]);
                        item.OrderNo = Convert.ToInt32(reader["OrderNo"]);
                        item.EnlargementFactor = float.Parse(reader["EnlargementFactor"] == DBNull.Value ? "0" : reader["EnlargementFactor"].ToString());
                        item.RateLimitTriggerIpCount = Convert.ToInt32(reader["RateLimitTriggerIpCount"]);
                        item.TableID = Convert.ToInt32(reader["Id"]);
                        item.ZoneId = Convert.ToString(reader["ZoneId"]);
                        item.RateLimitTriggerTime = Convert.ToInt32(reader["RateLimitTriggerTime"]);
                        item.IfBanIp = Convert.ToInt32(reader["IfBanIp"]) > 0;
                        item.IfOpenRateLimitRule = Convert.ToInt32(reader["IfOpenRateLimitRule"]) > 0;
                        item.IfTesting = Convert.ToInt32(reader["IfTesting"]) > 0;
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
                string query = @"SELECT Period, 
                                        Threshold, 
                                        Url, 
                                        OrderNo, 
                                        EnlargementFactor, 
                                        RateLimitTriggerIpCount, 
                                        Id, 
                                        ZoneId, 
                                        IfBanIp,
                                        IfOpenRateLimitRule,
                                        IfTesting,
                                        RateLimitTriggerTime FROM t_RateLimiting_Rules where Id=@id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        item.Period = Convert.ToInt32(reader["Period"]);
                        item.Threshold = Convert.ToInt32(reader["Threshold"]);
                        item.Url = Convert.ToString(reader["Url"]);
                        item.OrderNo = Convert.ToInt32(reader["OrderNo"]);
                        item.EnlargementFactor = float.Parse(reader["EnlargementFactor"] == DBNull.Value ? "0" : reader["EnlargementFactor"].ToString());
                        item.RateLimitTriggerIpCount = Convert.ToInt32(reader["RateLimitTriggerIpCount"]);
                        item.TableID = Convert.ToInt32(reader["Id"]);
                        item.ZoneId = Convert.ToString(reader["ZoneId"]);
                        item.RateLimitTriggerTime = Convert.ToInt32(reader["RateLimitTriggerTime"]);
                        item.IfBanIp = Convert.ToInt32(reader["IfBanIp"]) > 0;
                        item.IfOpenRateLimitRule = Convert.ToInt32(reader["IfOpenRateLimitRule"]) > 0;
                        item.IfTesting = Convert.ToInt32(reader["IfTesting"]) > 0;
                    }
                }
            }

            return item;
        }

        public static int GetRateLimitMaxOrder(string zoneId)
        {
            int count = 0;
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT MAX(OrderNo) 
                                    FROM dbo.t_RateLimiting_Rules
                                    WHERE ZoneId=@zoneId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                conn.Open();
                string counts = cmd.ExecuteScalar().ToString();
                count = string.IsNullOrWhiteSpace(counts) ? 0 : int.Parse(counts);
            }

            return count;
        }

    }
}
