using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Configuration;

namespace AttackPrevent.Access
{
    public class AuditLogAccess
    {
        public static List<AuditLogEntity> GetList(string zoneId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<AuditLogEntity>();
            var query = new StringBuilder(@"SELECT LogLevel, 
                                                   LogTime, 
                                                   LogOperator, 
                                                   Detail 
                                            FROM t_Logs
                                            WHERE ZoneId=@zoneID");
            if (startTime.HasValue)
            {
                query.Append(" AND LogTime >= @startTime ");
            }
            if (endTime.HasValue)
            {
                query.Append(" AND LogTime <= @endTime ");
            }
            if (!string.IsNullOrWhiteSpace(logType))
            {
                query.Append(" AND LogLevel IN (");
                logType = logType.Remove(logType.Length - 1);
                string[] ar = logType.Split(',');
                for (var i = 0; i < ar.Length; i++)
                {
                    query.Append("@logType" + i + ",");
                }
                query.Remove(query.Length - 1, 1);
                query.Append(") ");
            }
            if (!string.IsNullOrWhiteSpace(detail))
            {
                query.Append(" AND Detail LIKE'%'+@detail+'%' ");
            }
            query.Append("ORDER BY LogTime desc");
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
                    cmd.Parameters.AddWithValue("@endTime", endTime.Value.AddMinutes(1));
                }
                if (!string.IsNullOrWhiteSpace(logType))
                {
                    var ar = logType.Split(',');
                    for (var i = 0; i < ar.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@logType"+i, ar[i]);
                    }
                }
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    cmd.Parameters.AddWithValue("@detail", detail);
                }                

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var index = 1;
                    while (reader.Read())
                    {
                        var item = new AuditLogEntity
                        {
                            ID = index++,
                            LogType = Convert.ToString(reader["LogLevel"]),
                            LogTime = Convert.ToDateTime(reader["LogTime"]),
                            LogOperator = Convert.ToString(reader["LogOperator"]),
                            Detail = Convert.ToString(reader["Detail"])
                        };
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static void Add(AuditLogEntity item)
        {
            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (var conn = new SqlConnection(cons))
            {
                string query = @"INSERT INTO dbo.t_Logs
                                ( ZoneId ,
                                  LogLevel ,
                                  LogTime ,
                                  LogOperator ,
                                  IP ,
                                  Detail ,
                                  Remark
                                )
                        VALUES  ( @zoneID , 
                                  @logLevel ,
                                  @logTime ,
                                  @operator , 
                                  @ip ,
                                  @detail , 
                                  N''
                                )";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneID);
                cmd.Parameters.AddWithValue("@logLevel", item.LogType);
                cmd.Parameters.AddWithValue("@operator", item.LogOperator);
                cmd.Parameters.AddWithValue("@logTime", item.LogTime);
                cmd.Parameters.AddWithValue("@ip", item.IP);
                cmd.Parameters.AddWithValue("@detail", item.Detail);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void AddList(DataTable data)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (var conn = new SqlConnection(cons))
            {
                var bulkCopy = new SqlBulkCopy(conn)
                {
                    DestinationTableName = "t_Logs",
                    BatchSize = data.Rows.Count
                };
                conn.Open();
                bulkCopy.WriteToServer(data);
            }
        }

        private static void Add(AuditLogEntity log, SqlTransaction trans, SqlConnection conn)
        {
            string insertSql = @"INSERT INTO dbo.t_Logs
                                ( ZoneId ,
                                  LogLevel ,
                                  LogTime ,
                                  LogOperator ,
                                  IP ,
                                  Detail ,
                                  Remark
                                )
                        VALUES  ( @zoneID ,
                                  @logLevel ,
                                  @logTime ,
                                  @operator ,
                                  @ip , 
                                  @detail ,
                                  N''
                                )";
            var cmd = new SqlCommand(insertSql, conn, trans);
            cmd.Parameters.AddWithValue("@zoneID", log.ZoneID);
            cmd.Parameters.AddWithValue("@logLevel", log.LogType);
            cmd.Parameters.AddWithValue("@operator", log.LogOperator);
            cmd.Parameters.AddWithValue("@logTime", log.LogTime);
            cmd.Parameters.AddWithValue("@ip", log.IP);
            cmd.Parameters.AddWithValue("@detail", log.Detail);

            cmd.ExecuteNonQuery();
        }

        public static void Add(List<AuditLogEntity> logs)
        {
            var zoneId = logs[0].ZoneID;
            var connStr = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    foreach (var log in logs)
                    {
                        Add(log, tran, conn);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Add(new AuditLogEntity(zoneId, LogLevel.Error, $"Error when adding new log, \n eror message:{ex.Message} \n stack trace:{ex.StackTrace}"));
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}
