using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Access
{
    public class SmtpQueueAccess
    {
        public static List<SmtpQueue> GetList()
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<SmtpQueue> result = new List<SmtpQueue>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT Id,
                                        Title,
                                        Status,
                                        CreatedTime,
                                        SendedTime,
                                        Remark FROM t_Smtp_Queue";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SmtpQueue item = new SmtpQueue();
                        item.Id = Convert.ToInt32(reader["Id"]);
                        item.Title = Convert.ToString(reader["Title"]);
                        item.Status = Convert.ToInt32(reader["Status"]);               
                        item.CreatedTime = Convert.ToDateTime(reader["CreatedTime"]);
                        item.SendedTime = Convert.ToDateTime(reader["SendedTime"]);
                        item.Remark = Convert.ToString(reader["Remark"]);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static SmtpQueue GetByTitle(string title)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            SmtpQueue result = new SmtpQueue();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT Id,
                                        Title,
                                        Status,
                                        CreatedTime,
                                        SendedTime,
                                        Remark FROM t_Smtp_Queue WHERE Title=@title";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@title", title);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Id = Convert.ToInt32(reader["Id"]);
                        result.Title = Convert.ToString(reader["Title"]);
                        result.Status = Convert.ToInt32(reader["Status"]);
                        result.CreatedTime = Convert.ToDateTime(reader["CreatedTime"]);
                        result.SendedTime = Convert.ToDateTime(reader["SendedTime"]);
                        result.Remark = Convert.ToString(reader["Remark"]);
                    }
                }
            }

            return result;
        }

        public static void Add(SmtpQueue item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            
            StringBuilder query = new StringBuilder(@"INSERT INTO dbo.t_Smtp_Queue
                                                        ( Title,
                                                          Status,
                                                          CreatedTime,
                                                          SendedTime,
                                                          Remark
                                                        )
                                                VALUES  ( @title,
                                                          @status,                                                     
                                                          @createdTime,
                                                          @sendedTime,
                                                          @remark
                                                        )");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@status", item.Status);
                cmd.Parameters.AddWithValue("@createdTime", item.CreatedTime);
                cmd.Parameters.AddWithValue("@sendedTime", item.SendedTime);
                cmd.Parameters.AddWithValue("@remark", item.Remark);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(SmtpQueue item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"UPDATE dbo.t_Smtp_Queue 
                                                      SET Title=@title,
                                                          Status=@status,                                                  
                                                          CreatedTime=@createdTime,
                                                          SendedTime=@sendedTime,
                                                          Remark=@remark WHERE Id=@id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@status", item.Status);
                cmd.Parameters.AddWithValue("@createdTime", item.CreatedTime);
                cmd.Parameters.AddWithValue("@sendedTime", item.SendedTime);
                cmd.Parameters.AddWithValue("@remark", item.Remark);
                cmd.Parameters.AddWithValue("@id", item.Id);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(string title)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"DELETE dbo.t_Smtp_Queue 
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
