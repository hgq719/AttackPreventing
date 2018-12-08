using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class ActionReportAccess
    {
        public static List<ActionReport> GetListByTitle(string title)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<ActionReport> result = new List<ActionReport>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT Id,
                                        Title,
                                        ZoneId,
                                        IP,
                                        HostName,
                                        Max,
                                        Min,
                                        Avg,
                                        FullUrl,
                                        CreatedTime,
                                        Mode,
                                        Count,
                                        MaxDisplay,
                                        MinDisplay,
                                        AvgDisplay,
                                        Remark FROM t_Action_Report WHERE Title=@title";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@title", title);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ActionReport item = new ActionReport();
                        item.Id = Convert.ToInt32(reader["Id"]);
                        item.Title = Convert.ToString(reader["Title"]);
                        item.ZoneId = Convert.ToString(reader["ZoneId"]);
                        item.IP = Convert.ToString(reader["IP"]);
                        item.HostName = Convert.ToString(reader["HostName"]);
                        item.Max = Convert.ToInt32(reader["Max"]);
                        item.Min = Convert.ToInt32(reader["Min"]);
                        item.Avg = Convert.ToInt32(reader["Avg"]);
                        item.FullUrl = Convert.ToString(reader["FullUrl"]);
                        item.CreatedTime = Convert.ToDateTime(reader["CreatedTime"]);
                        item.Mode = Convert.ToString(reader["Mode"]);
                        item.Count = Convert.ToInt32(reader["Count"]);
                        item.Remark = Convert.ToString(reader["Remark"]);
                        item.MaxDisplay = Convert.ToString(reader["MaxDisplay"]);
                        item.MinDisplay = Convert.ToString(reader["MinDisplay"]);
                        item.AvgDisplay = Convert.ToString(reader["AvgDisplay"]);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static List<ActionReport> GetListByIp(string ip)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<ActionReport> result = new List<ActionReport>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT Id,
                                        Title,
                                        ZoneId,
                                        IP,
                                        HostName,
                                        Max,
                                        Min,
                                        Avg,
                                        FullUrl,
                                        CreatedTime,
                                        Mode,
                                        Count,
                                        MaxDisplay,
                                        MinDisplay,
                                        AvgDisplay,
                                        Remark FROM t_Action_Report WHERE IP=@ip ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ip", ip);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ActionReport item = new ActionReport();
                        item.Id = Convert.ToInt32(reader["Id"]);
                        item.Title = Convert.ToString(reader["Title"]);
                        item.ZoneId = Convert.ToString(reader["ZoneId"]);
                        item.IP = Convert.ToString(reader["IP"]);
                        item.HostName = Convert.ToString(reader["HostName"]);
                        item.Max = Convert.ToInt32(reader["Max"]);
                        item.Min = Convert.ToInt32(reader["Min"]);
                        item.Avg = Convert.ToInt32(reader["Avg"]);
                        item.FullUrl = Convert.ToString(reader["FullUrl"]);
                        item.CreatedTime = Convert.ToDateTime(reader["CreatedTime"]);
                        item.Mode = Convert.ToString(reader["Mode"]);
                        item.Count = Convert.ToInt32(reader["Count"]);
                        item.Remark = Convert.ToString(reader["Remark"]);
                        item.MaxDisplay = Convert.ToString(reader["MaxDisplay"]);
                        item.MinDisplay = Convert.ToString(reader["MinDisplay"]);
                        item.AvgDisplay = Convert.ToString(reader["AvgDisplay"]);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static void Add(ActionReport item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            
            StringBuilder query = new StringBuilder(@"INSERT INTO dbo.t_Action_Report
                                                        ( Title,
                                                          ZoneId,
                                                          IP,
                                                          HostName,
                                                          Max,
                                                          Min,
                                                          Avg,
                                                          FullUrl,
                                                          CreatedTime,
                                                          Mode,
                                                          Count,
                                                          Remark,
                                                          MaxDisplay,
                                                          MinDisplay,
                                                          AvgDisplay
                                                        )
                                                VALUES  ( @title,
                                                          @zoneId,
                                                          @ip,
                                                          @hostName,
                                                          @max,
                                                          @min,
                                                          @avg,
                                                          @fullUrl,
                                                          @createdTime,
                                                          @mode,
                                                          @count,
                                                          @remark,
                                                          @maxDisplay,
                                                          @minDisplay,
                                                          @avgDisplay
                                                        )");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@zoneId", item.ZoneId);
                cmd.Parameters.AddWithValue("@ip", item.IP);
                cmd.Parameters.AddWithValue("@hostName", item.HostName);
                cmd.Parameters.AddWithValue("@max", item.Max);
                cmd.Parameters.AddWithValue("@min", item.Min);
                cmd.Parameters.AddWithValue("@avg", item.Avg);
                cmd.Parameters.AddWithValue("@fullUrl", item.FullUrl);
                cmd.Parameters.AddWithValue("@createdTime", item.CreatedTime);
                cmd.Parameters.AddWithValue("@mode", item.Mode);
                cmd.Parameters.AddWithValue("@count", item.Count);
                cmd.Parameters.AddWithValue("@remark", item.Remark);
                cmd.Parameters.AddWithValue("@maxDisplay", item.MaxDisplay);
                cmd.Parameters.AddWithValue("@minDisplay", item.MinDisplay);
                cmd.Parameters.AddWithValue("@avgDisplay", item.AvgDisplay);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(ActionReport item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"UPDATE dbo.t_Action_Report 
                                                      SET Title=@title,
                                                          ZoneId=@zoneId,
                                                          IP=@ip,
                                                          HostName=@hostName,
                                                          Max=@max,
                                                          Min=@min,
                                                          Avg=@avg,
                                                          FullUrl=@fullUrl,
                                                          CreatedTime=@createdTime,
                                                          Mode=@mode,
                                                          Count=@count,
                                                          MaxDisplay=@maxDisplay,
                                                          MinDisplay=@minDisplay,
                                                          AvgDisplay=@avgDisplay,
                                                          Remark=@remark WHERE Id=@id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@zoneId", item.ZoneId);
                cmd.Parameters.AddWithValue("@ip", item.IP);
                cmd.Parameters.AddWithValue("@hostName", item.HostName);
                cmd.Parameters.AddWithValue("@max", item.Max);
                cmd.Parameters.AddWithValue("@min", item.Min);
                cmd.Parameters.AddWithValue("@avg", item.Avg);
                cmd.Parameters.AddWithValue("@fullUrl", item.FullUrl);
                cmd.Parameters.AddWithValue("@createdTime", item.CreatedTime);
                cmd.Parameters.AddWithValue("@mode", item.Mode);
                cmd.Parameters.AddWithValue("@count", item.Count);
                cmd.Parameters.AddWithValue("@remark", item.Remark);
                cmd.Parameters.AddWithValue("@maxDisplay", item.MaxDisplay);
                cmd.Parameters.AddWithValue("@minDisplay", item.MinDisplay);
                cmd.Parameters.AddWithValue("@avgDisplay", item.AvgDisplay);
                cmd.Parameters.AddWithValue("@id", item.Id);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(string title)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"DELETE dbo.t_Action_Report 
                                                      WHERE Title=@title");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@title", title);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

    }
}
