using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class ZoneAccess
    {
        public static List<ZoneEntity> GetAllList()
        {
            var cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<ZoneEntity>();
            using (var conn = new SqlConnection(cons))
            {
                const string query = @"SELECT [ZoneId],
                                        [ZoneName],
                                        [AuthEmail],
                                        [IfTestStage],
                                        [IfEnable],
                                        [IfAttacking],
                                        [ThresholdForHost],
                                        [PeriodForHost],
                                        [IfAnalyzeByHostRule],
                                        [AuthKey] FROM [t_Zone_Info] ";
                var cmd = new SqlCommand(query, conn);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ZoneEntity
                        {
                            ZoneId = Convert.ToString(reader["ZoneId"]),
                            ZoneName = Convert.ToString(reader["ZoneName"]),
                            AuthEmail = Convert.ToString(reader["AuthEmail"]),
                            IfTestStage = Convert.ToInt32(reader["IfTestStage"]) > 0,
                            IfEnable = Convert.ToInt32(reader["IfEnable"]) > 0,
                            IfAttacking = Convert.ToInt32(reader["IfAttacking"]) > 0,
                            AuthKey = Convert.ToString(reader["AuthKey"]),
                            ThresholdForHost = Convert.ToInt32(reader["ThresholdForHost"]),
                            PeriodForHost = Convert.ToInt32(reader["PeriodForHost"]),
                            IfAnalyzeByHostRule = Convert.ToInt32(reader["IfAnalyzeByHostRule"]) > 0
                        });
                    }
                }
            }

            return result;
        }

        public static List<ZoneEntity> GetList(string zoneID, string zoneName, bool ifTest, bool ifEnabel)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<ZoneEntity> result = new List<ZoneEntity>();
            StringBuilder query = new StringBuilder(@"SELECT [ZoneId],
                                                             [ZoneName],
                                                             [AuthEmail],
                                                             [IfTestStage],
                                                             [IfEnable],
                                                             [IfAttacking],
                                                             [ThresholdForHost],
                                                             [PeriodForHost],
                                                             [IfAnalyzeByHostRule],
                                                             [Id] FROM [t_Zone_Info] ");
            StringBuilder where = new StringBuilder();
            //if (!string.IsNullOrWhiteSpace(zoneID))
            //{
            //    where.Append(" ZoneId LIKE'%'+@zoneID+'%' AND ");
            //}
            if (!string.IsNullOrWhiteSpace(zoneName))
            {
                where.Append(" ZoneName LIKE'%'+@zoneName+'%' AND ");
            }
            //if (ifTest)
            //{
            //    where.Append(" IfTestStage = @ifTest AND ");
            //}
            //if (ifEnabel)
            //{
            //    where.Append(" IfEnable = @ifEnabel AND ");
            //}
            if (where.Length > 4)
            {
                where.Remove(where.Length - 4, 4);
                query.AppendFormat(" WHERE {0}", where.ToString());
            }

            using (SqlConnection conn = new SqlConnection(cons))
            {
                
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                //if (!string.IsNullOrWhiteSpace(zoneID))
                //{
                //    cmd.Parameters.AddWithValue("@zoneID", zoneID);
                //}
                if (!string.IsNullOrWhiteSpace(zoneName))
                {
                    cmd.Parameters.AddWithValue("@zoneName", zoneName);
                }
                //if (ifTest)
                //{
                //    cmd.Parameters.AddWithValue("@ifTest", ifTest);
                //}
                //if (ifEnabel)
                //{
                //    cmd.Parameters.AddWithValue("@ifEnabel", ifEnabel);
                //}
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        ZoneEntity item = new ZoneEntity();
                        item.ID = index++;
                        item.ZoneId = Convert.ToString(reader["ZoneId"]);
                        item.ZoneName = Convert.ToString(reader["ZoneName"]);
                        item.AuthEmail = Convert.ToString(reader["AuthEmail"]);
                        item.IfTestStage = Convert.ToInt32(reader["IfTestStage"]) > 0;
                        item.IfEnable = Convert.ToInt32(reader["IfEnable"]) > 0;
                        item.IfAttacking = Convert.ToInt32(reader["IfAttacking"]) > 0;
                        item.TableID = Convert.ToInt32(reader["Id"]);
                        item.ThresholdForHost = Convert.ToInt32(reader["ThresholdForHost"]);
                        item.PeriodForHost = Convert.ToInt32(reader["PeriodForHost"]);
                        item.IfAnalyzeByHostRule = Convert.ToInt32(reader["IfAnalyzeByHostRule"]) > 0;
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static void Add(ZoneEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            
            StringBuilder query = new StringBuilder(@"INSERT INTO dbo.t_Zone_Info
                                                        ( ZoneId ,
                                                          ZoneName ,
                                                          AuthEmail ,
                                                          AuthKey ,
                                                          IfTestStage ,
                                                          IfEnable ,
                                                          IfAttacking ,
                                                          ThresholdForHost,
                                                          PeriodForHost,
                                                          IfAnalyzeByHostRule
                                                        )
                                                VALUES  ( @zoneID , -- ZoneId - nvarchar(512)
                                                          @zoneName , -- ZoneName - nvarchar(256)
                                                          @authEmail , -- AuthEmail - nvarchar(256)
                                                          @authKey , -- AuthKey - nvarchar(256)
                                                          @ifTest , -- IfTestStage - int
                                                          @ifEnable , -- IfEnable - int
                                                          @ifAttacking ,  -- IfAttacking - int
                                                          @thresholdForHost ,  -- ThresholdForHost - int
                                                          @periodForHost ,  -- PeriodForHost - int                                                          
                                                          @ifAnalyzeByHostRule ,  -- IfAnalyzeByHostRule - int
                                                        )");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@zoneName", item.ZoneName);
                cmd.Parameters.AddWithValue("@authEmail", item.AuthEmail);
                cmd.Parameters.AddWithValue("@authKey", item.AuthKey);
                cmd.Parameters.AddWithValue("@ifTest", item.IfTestStage);
                cmd.Parameters.AddWithValue("@ifEnable", item.IfEnable);
                cmd.Parameters.AddWithValue("@ifAttacking", item.IfAttacking);
                cmd.Parameters.AddWithValue("@thresholdForHost", item.ThresholdForHost);
                cmd.Parameters.AddWithValue("@periodForHost", item.PeriodForHost);
                cmd.Parameters.AddWithValue("@ifAnalyzeByHostRule", item.IfAnalyzeByHostRule);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(ZoneEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"UPDATE dbo.t_Zone_Info 
                                                        SET ZoneId=@zoneID,
                                                            ZoneName=@zoneName,
                                                            AuthEmail=@authEmail,
                                                            AuthKey=@authKey,
                                                            IfTestStage=@ifTest,
                                                            IfEnable=@ifEnable,
                                                            ThresholdForHost=@thresholdForHost,
                                                            PeriodForHost=@periodForHost,
                                                            IfAnalyzeByHostRule=@ifAnalyzeByHostRule,
                                                            IfAttacking=@ifAttacking WHERE Id=@id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@zoneName", item.ZoneName);
                cmd.Parameters.AddWithValue("@authEmail", item.AuthEmail);
                cmd.Parameters.AddWithValue("@authKey", item.AuthKey);
                cmd.Parameters.AddWithValue("@ifTest", item.IfTestStage);
                cmd.Parameters.AddWithValue("@ifEnable", item.IfEnable);
                cmd.Parameters.AddWithValue("@ifAttacking", item.IfAttacking);
                cmd.Parameters.AddWithValue("@id", item.TableID);
                cmd.Parameters.AddWithValue("@thresholdForHost", item.ThresholdForHost);
                cmd.Parameters.AddWithValue("@periodForHost", item.PeriodForHost);
                cmd.Parameters.AddWithValue("@ifAnalyzeByHostRule", item.IfAnalyzeByHostRule);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static ZoneEntity GetZone(int id)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            ZoneEntity result = new ZoneEntity();
            StringBuilder query = new StringBuilder(@"SELECT [ZoneId],
                                                             [ZoneName],
                                                             [AuthEmail],
                                                             [IfTestStage],
                                                             [IfEnable],
                                                             [IfAttacking], 
                                                             [ThresholdForHost],
                                                             [PeriodForHost],
                                                             [IfAnalyzeByHostRule],
                                                             [Id], 
                                                             [AuthKey] from [t_Zone_Info] WHERE [Id] = @id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                    cmd.Parameters.AddWithValue("@id", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new ZoneEntity();
                        result.ZoneId = Convert.ToString(reader["ZoneId"]);
                        result.ZoneName = Convert.ToString(reader["ZoneName"]);
                        result.AuthEmail = Convert.ToString(reader["AuthEmail"]);
                        result.IfTestStage = Convert.ToInt32(reader["IfTestStage"]) > 0;
                        result.IfEnable = Convert.ToInt32(reader["IfEnable"]) > 0;
                        result.IfAttacking = Convert.ToInt32(reader["IfAttacking"]) > 0;
                        result.TableID = Convert.ToInt32(reader["Id"]);
                        result.AuthKey = Convert.ToString(reader["AuthKey"]);
                        result.ThresholdForHost = Convert.ToInt32(reader["ThresholdForHost"]);
                        result.PeriodForHost = Convert.ToInt32(reader["PeriodForHost"]);
                        result.IfAnalyzeByHostRule = Convert.ToInt32(reader["IfAnalyzeByHostRule"]) > 0;
                    }
                }
            }

            return result;
        }

        public static ZoneEntity GetZone(string zoneID, string zoneName)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            ZoneEntity result = new ZoneEntity();
            StringBuilder query = new StringBuilder(@"SELECT [ZoneId],
                                                             [ZoneName],
                                                             [AuthEmail],
                                                             [IfTestStage],
                                                             [IfEnable],
                                                             [IfAttacking], 
                                                             [Id], 
                                                             [ThresholdForHost],
                                                             [PeriodForHost],
                                                             [IfAnalyzeByHostRule],
                                                             [AuthKey] from [t_Zone_Info] WHERE [ZoneId] LIKE'%'+@zoneID+'%' or [ZoneName] LIKE'%'+@zoneName+'%'");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneID);
                cmd.Parameters.AddWithValue("@zoneName", zoneName);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new ZoneEntity();
                        result.ZoneId = Convert.ToString(reader["ZoneId"]);
                        result.ZoneName = Convert.ToString(reader["ZoneName"]);
                        result.AuthEmail = Convert.ToString(reader["AuthEmail"]);
                        result.IfTestStage = Convert.ToInt32(reader["IfTestStage"]) > 0;
                        result.IfEnable = Convert.ToInt32(reader["IfEnable"]) > 0;
                        result.IfAttacking = Convert.ToInt32(reader["IfAttacking"]) > 0;
                        result.TableID = Convert.ToInt32(reader["Id"]);
                        result.AuthKey = Convert.ToString(reader["AuthKey"]);
                        result.ThresholdForHost = Convert.ToInt32(reader["ThresholdForHost"]);
                        result.PeriodForHost = Convert.ToInt32(reader["PeriodForHost"]);
                        result.IfAnalyzeByHostRule = Convert.ToInt32(reader["IfAnalyzeByHostRule"]) > 0;
                    }
                }
            }

            return result;
        }

        public static bool UpdateAttackFlag(bool ifAttacking, string zoneId)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"UPDATE T_ZONE_INFO 
                                    SET IfAttacking = @IfAttacking WHERE ZoneId = @ZoneId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ZoneId", zoneId);
                cmd.Parameters.AddWithValue("@IfAttacking", ifAttacking ? 1 : 0);
                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool Equals(string zoneId, int id)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT COUNT(1)
                                    FROM dbo.t_Zone_Info
                                    WHERE ZoneId= @zoneId ";
                if (id > 0)
                {
                    query += " AND Id <> @id"; 
                }
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                if (id > 0)
                {
                    cmd.Parameters.AddWithValue("@id", id);
                }
                conn.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
