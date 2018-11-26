using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class AuditLogAccess
    {
        public static List<AuditLogEntity> GetList(string zoneID, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<AuditLogEntity> result = new List<AuditLogEntity>();
            StringBuilder query = new StringBuilder("select LogLevel, LogTime, LogOperator, Detail from t_Logs where ZoneId=@zoneID");
            if (startTime.HasValue)
            {
                query.Append(" and LogTime >= @startTime ");
            }
            if (endTime.HasValue)
            {
                query.Append(" and LogTime <= @endTime ");
            }
            if (!string.IsNullOrWhiteSpace(logType))
            {
                query.Append(" and LogLevel in (");
                logType = logType.Remove(logType.Length - 1);
                string[] ar = logType.Split(',');
                for (int i = 0; i < ar.Length; i++)
                {
                    query.Append("@logType" + i + ",");
                }
                query.Remove(query.Length - 1, 1);
                query.Append(") ");
                //logType = "'" + logType + "'";
                //logType = logType.Replace(",", "','");
                //query.AppendFormat(" and LogLevel in ({0}) ", logType);
            }
            if (!string.IsNullOrWhiteSpace(detail))
            {
                query.Append(" and Detail like'%'+@detail+'%'");
            }
            using (SqlConnection conn = new SqlConnection(cons))
            {
                
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", zoneID);
                if (startTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@startTime", startTime);
                }
                if (endTime.HasValue)
                {
                    cmd.Parameters.AddWithValue("@endTime", endTime);
                }
                if (!string.IsNullOrWhiteSpace(logType))
                {
                    string[] ar = logType.Split(',');
                    for (int i = 0; i < ar.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@logType"+i, ar[i]);
                    }
                }
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    cmd.Parameters.AddWithValue("@detail", detail);
                }                

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
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

        public static void Add(AuditLogEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
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
VALUES  ( @zoneID , -- ZoneId - nvarchar(512)
          @logLevel , -- LogLevel - nvarchar(256)
          GETDATE() , -- LogTime - datetime
          @operator , -- LogOperator - nvarchar(256)
          @ip , -- IP - nvarchar(256)
          @detail , -- Detail - nvarchar(max)
          N''  -- Remark - nvarchar(1024)
        )";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneID);
                cmd.Parameters.AddWithValue("@logLevel", item.LogType);
                cmd.Parameters.AddWithValue("@operator", item.LogOperator);
                cmd.Parameters.AddWithValue("@ip", item.IP);
                cmd.Parameters.AddWithValue("@detail", item.Detail);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void AddList(DataTable data)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                var bulkCopy = new SqlBulkCopy(conn);
                bulkCopy.DestinationTableName = "t_Logs";
                bulkCopy.BatchSize = data.Rows.Count;
                conn.Open();
                bulkCopy.WriteToServer(data);
            }
        }
    }
}
