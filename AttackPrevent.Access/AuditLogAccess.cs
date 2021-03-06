﻿using AttackPrevent.Model;
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
        public static List<AuditLogEntity> GetList(int zoneTableId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {

            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<AuditLogEntity>();
            var query = new StringBuilder(@"SELECT LogLevel, 
                                                   LogTime, 
                                                   LogOperator, 
                                                   Detail 
                                            FROM t_Logs WITH(NOLOCK) ");
            var where = new StringBuilder();
            
            if (startTime.HasValue)
            {
                where.Append(" LogTime >= @startTime AND ");
            }
            if (endTime.HasValue)
            {
                where.Append(" LogTime <= @endTime AND ");
            }
            where.Append(" ZoneTableId=@zoneTableId AND ");
            if (!string.IsNullOrWhiteSpace(logType))
            {
                where.Append(" LogLevel IN (");
                logType = logType.Remove(logType.Length - 1);
                string[] ar = logType.Split(',');
                for (var i = 0; i < ar.Length; i++)
                {
                    where.Append("@logType" + i + ",");
                }
                where.Remove(where.Length - 1, 1);
                where.Append(") AND ");
            }
            if (!string.IsNullOrWhiteSpace(detail))
            {
                detail = detail.Replace("[", "@[");
                detail = detail.Replace("%", "@%");
                detail = detail.Replace("]", "@]");
                detail = detail.Replace("^", "@^");
                detail = detail.Replace("-", "@-");
                detail = detail.Replace("_", "@_");
                where.Append(" Detail LIKE'%'+@detail+'%' ESCAPE '@' AND ");
            }
            if (where.Length > 0)
            {
                where.Remove(where.Length - 4, 4);
            }
            query.AppendFormat(" WHERE {0}", where.ToString());
            query.Append("ORDER BY LogTime desc");
            using (var conn = new SqlConnection(cons))
            {
                
                var cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 2 * 60;
                cmd.Parameters.AddWithValue("@zoneTableId", zoneTableId);             
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
                            LogType = (LogLevel)Convert.ToInt32(reader["LogLevel"]),
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

        public static List<AuditLogEntity> GetListByPage(int offset, int limit, int zoneTableId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {

            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<AuditLogEntity>();
            var query = new StringBuilder(@"SELECT LogLevel, 
                                                   LogTime, 
                                                   LogOperator, 
                                                   Detail 
                                            FROM t_Logs WITH(NOLOCK) ");
            var where = new StringBuilder();

            if (startTime.HasValue)
            {
                where.Append(" LogTime >= @startTime AND ");
            }
            if (endTime.HasValue)
            {
                where.Append(" LogTime <= @endTime AND ");
            }
            where.Append(" ZoneTableId=@zoneTableId AND ");

            if (string.IsNullOrWhiteSpace(logType))
            {
                logType = "0,2,1";
            }
            else
            {
                logType = logType.Remove(logType.Length - 1);
            }

            if (logType.Length == 1)
            {
                where.Append(" LogLevel = @logType AND ");
            }
            else if (logType.Length > 1)
            {
                where.Append(" LogLevel IN (");

                string[] ar = logType.Split(',');
                for (var i = 0; i < ar.Length; i++)
                {
                    where.Append("@logType" + i + ",");
                }
                where.Remove(where.Length - 1, 1);
                where.Append(") AND ");
            }

            //where.Append(" (");
            //logType = logType.Remove(logType.Length - 1);
            //string[] ar = logType.Split(',');
            //for (var i = 0; i < ar.Length; i++)
            //{
            //    where.AppendFormat("LogLevel = @logType" + i + " OR ");
            //}
            //where.Remove(where.Length - 3, 3);
            //where.Append(") AND ");


            //if (!string.IsNullOrWhiteSpace(logType))
            //{
            //    where.Append(" LogLevel IN (");
            //    logType = logType.Remove(logType.Length - 1);
            //    string[] ar = logType.Split(',');
            //    for (var i = 0; i < ar.Length; i++)
            //    {
            //        where.Append("@logType" + i + ",");
            //    }
            //    where.Remove(where.Length - 1, 1);
            //    where.Append(") AND ");
            //}
            //else
            //{
            //    where.Append(" LogLevel IN ('0', '2', '1') AND ");
            //}

            if (!string.IsNullOrWhiteSpace(detail))
            {
                detail = detail.Replace("[", "@[");
                detail = detail.Replace("%", "@%");
                detail = detail.Replace("]", "@]");
                detail = detail.Replace("^", "@^");
                detail = detail.Replace("-", "@-");
                detail = detail.Replace("_", "@_"); 
                where.Append(" Detail LIKE'%'+@detail+'%' ESCAPE '@' AND ");
            }
            if (where.Length > 0)
            {
                where.Remove(where.Length - 4, 4);
            }
            query.AppendFormat(" WHERE {0}", where.ToString());
            query.Append("ORDER BY logtime desc offset @offset rows fetch next @limit rows only");
            using (var conn = new SqlConnection(cons))
            {

                var cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 2 * 60;
                cmd.Parameters.AddWithValue("@zoneTableId", zoneTableId);
                if (startTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@startTime", startTime.Value);
                }
                if (endTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@endTime", endTime.Value.AddMinutes(1));
                }

                if (logType.Length == 1)
                {
                    cmd.Parameters.AddWithValue("@logType", Convert.ToInt32(logType));
                }
                else if (logType.Length > 1)
                {
                    var ar = logType.Split(',');
                    for (var i = 0; i < ar.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@logType" + i, ar[i]);
                    }
                }

                //if (!string.IsNullOrWhiteSpace(logType))
                //{
                //    var ar = logType.Split(',');
                //    for (var i = 0; i < ar.Length; i++)
                //    {
                //        cmd.Parameters.AddWithValue("@logType" + i, Convert.ToInt32(ar[i]));
                //    }
                //}

                if (!string.IsNullOrWhiteSpace(detail))
                {
                    cmd.Parameters.AddWithValue("@detail", detail);
                }
                cmd.Parameters.AddWithValue("@offset", offset);
                cmd.Parameters.AddWithValue("@limit", limit);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var index = 1;
                    while (reader.Read())
                    {
                        var item = new AuditLogEntity
                        {
                            ID = index++,
                            LogType = (LogLevel)Convert.ToInt32(reader["LogLevel"]),
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

        public static int GetCountByPage(int zoneTableId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            
            var query = new StringBuilder(@"SELECT count(1) FROM t_Logs WITH(NOLOCK) ");
            var where = new StringBuilder();

            if (startTime.HasValue)
            {
                where.Append(" LogTime >= @startTime AND ");
            }
            if (endTime.HasValue)
            {
                where.Append(" LogTime <= @endTime AND ");
            }
            where.Append(" ZoneTableId=@zoneTableId AND ");
            if (!string.IsNullOrWhiteSpace(logType))
            {
                where.Append(" LogLevel IN (");
                logType = logType.Remove(logType.Length - 1);
                string[] ar = logType.Split(',');
                for (var i = 0; i < ar.Length; i++)
                {
                    where.Append("@logType" + i + ",");
                }
                where.Remove(where.Length - 1, 1);
                where.Append(") AND ");
            }
            if (!string.IsNullOrWhiteSpace(detail))
            {
                detail = detail.Replace("[", "@[");
                detail = detail.Replace("%", "@%");
                detail = detail.Replace("]", "@]");
                detail = detail.Replace("^", "@^");
                detail = detail.Replace("-", "@-");
                detail = detail.Replace("_", "@_");
                where.Append(" Detail LIKE'%'+@detail+'%' ESCAPE '@' AND ");
            }
            if (where.Length > 0)
            {
                where.Remove(where.Length - 4, 4);
            }
            query.AppendFormat(" WHERE {0}", where.ToString());
            
            using (var conn = new SqlConnection(cons))
            {

                var cmd = new SqlCommand(query.ToString(), conn);
                cmd.CommandTimeout = 2 * 60;
                cmd.Parameters.AddWithValue("@zoneTableId", zoneTableId);
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
                        cmd.Parameters.AddWithValue("@logType" + i, ar[i]);
                    }
                }
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    cmd.Parameters.AddWithValue("@detail", detail);
                }

                conn.Open();

                return (int)cmd.ExecuteScalar();
            }

        }

        public static void Add(AuditLogEntity item)
        {
           
            if (item.ZoneTableID == 0) { item.ZoneTableID = ZoneAccess.GetZoneByZoneId(item.ZoneID).TableID; }
            try
            {
                var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

                using (var conn = new SqlConnection(cons))
                {
                    const string query = @"INSERT INTO dbo.t_Logs
                                ( ZoneTableId ,
                                  LogLevel ,
                                  LogTime ,
                                  LogOperator ,
                                  IP ,
                                  Detail ,
                                  Remark
                                )
                        VALUES  ( @zoneTableId , 
                                  @logLevel ,
                                  @logTime ,
                                  @operator , 
                                  @ip ,
                                  @detail , 
                                  N''
                                )";
                    var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@zoneTableId", item.ZoneTableID);
                    cmd.Parameters.AddWithValue("@logLevel", item.LogType);
                    cmd.Parameters.AddWithValue("@operator", item.LogOperator);
                    cmd.Parameters.AddWithValue("@logTime", item.LogTime);
                    cmd.Parameters.AddWithValue("@ip", item.IP);
                    cmd.Parameters.AddWithValue("@detail", item.Detail);
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when adding new log, error message:{ex.Message} \n stack trace:{ex.StackTrace}");
            }
            
        }

        public static void AddList(DataTable data)
        {
            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

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
            const string insertSql = @"INSERT INTO dbo.t_Logs
                                ( ZoneTableId ,
                                  LogLevel ,
                                  LogTime ,
                                  LogOperator ,
                                  IP ,
                                  Detail ,
                                  Remark
                                )
                        VALUES  ( @zoneTableId ,
                                  @logLevel ,
                                  @logTime ,
                                  @operator ,
                                  @ip , 
                                  @detail ,
                                  N''
                                )";
            var cmd = new SqlCommand(insertSql, conn, trans);
            cmd.Parameters.AddWithValue("@zoneTableId", log.ZoneTableID);
            cmd.Parameters.AddWithValue("@logLevel", log.LogType);
            cmd.Parameters.AddWithValue("@operator", log.LogOperator);
            cmd.Parameters.AddWithValue("@logTime", log.LogTime);
            cmd.Parameters.AddWithValue("@ip", log.IP);
            cmd.Parameters.AddWithValue("@detail", log.Detail);

            cmd.ExecuteNonQuery();
        }

        public static void Add(List<AuditLogEntity> logs)
        {
            var zoneId = logs[0].ZoneTableID;
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
                    Add(new AuditLogEntity(zoneId, LogLevel.Error, $"Error when adding new log, \n error message:{ex.Message} \n stack trace:{ex.StackTrace}"));
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}
