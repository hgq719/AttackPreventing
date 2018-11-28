﻿using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace AttackPrevent.Access
{
    public class BanIpHistoryAccess
    {
        public static List<BanIpHistory> Get(string zoneId, string ip = null)
        {
            var cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (var conn = new SqlConnection(cons))
            {
                var sbSql = new StringBuilder("SELECT [ID], [ZoneId], [IP], [LatestTriggerTime], [RuleId], [Remark] FROM T_Ban_IP_History WHERE ZoneId = @ZoneId");

                var cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@ZoneId", zoneId);
                if (!string.IsNullOrEmpty(ip))
                {
                    sbSql.Append(" And IP = @IP ");
                    cmd.Parameters.AddWithValue("@IP",ip);
                }
                cmd.Connection = conn;
                cmd.CommandText = sbSql.ToString();
                conn.Open();

                var banIPHistories = new List<BanIpHistory>();
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        banIPHistories.Add(new BanIpHistory()
                        {
                            Id = Convert.ToInt32(reader["ID"]),
                            IP = ip,
                            LatestTriggerTime = Convert.ToDateTime(reader["LatestTriggerTime"]),
                            ZoneId = Convert.ToString(reader["ZoneId"]),
                            RuleId = Convert.ToInt32(reader["RuleId"]),
                            Remark = Convert.ToString(reader["Remark"])
                        });
                    }
                }

                return banIPHistories;
            }

        }

        public static bool Add(BanIpHistory banIPHistory)
        {
            var connStr = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            var strSql = @"INSERT INTO T_Ban_IP_History(
                                                [ZoneId],
                                                [IP],
                                                [LatestTriggerTime],
                                                [RuleId], 
                                                [Remark])
                                             VALUES (
                                                @ZoneId,
                                                @IP,
                                                GETUTCDATE(),
                                                @RuleId,
                                                @Remark)";

            using (var conn = new SqlConnection(connStr))
            {

                var cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@ZoneId", banIPHistory.ZoneId);
                cmd.Parameters.AddWithValue("@IP", banIPHistory.IP);
                cmd.Parameters.AddWithValue("@RuleId", banIPHistory.RuleId);
                cmd.Parameters.AddWithValue("@Remark", banIPHistory.Remark);
                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool Update(BanIpHistory banIPHistory)
        {
            var connStr = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            var strSql = @"UPDATE T_Ban_IP_History 
                           SET LatestTriggerTime = GETUTCDATE(), 
                               RuleId = @RuleId, 
                               Remark = @Remark
                           WHERE ZoneId = @ZoneId And IP = @IP";

            using (var conn = new SqlConnection(connStr))
            {

                var cmd = new SqlCommand(strSql, conn);
                cmd.Parameters.AddWithValue("@ZoneId", banIPHistory.ZoneId);
                cmd.Parameters.AddWithValue("@IP", banIPHistory.IP);
                cmd.Parameters.AddWithValue("@RuleId", banIPHistory.RuleId);
                cmd.Parameters.AddWithValue("@Remark", banIPHistory.Remark);
                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}