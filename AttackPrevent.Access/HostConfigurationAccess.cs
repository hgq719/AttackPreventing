using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace AttackPrevent.Access
{
    public class HostConfigurationAccess
    {
        public static List<HostConfigurationEntity> GetList()
        {
            string connString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            var result = new List<HostConfigurationEntity>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT [Id], 
                                        [Host], 
                                        [Threshold], 
                                        [Period] FROM [t_Host_Configuration]";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new HostConfigurationEntity
                        {
                            ID = Convert.ToInt32(reader["Id"]),
                            Host = Convert.ToString(reader["Host"]),
                            Threshold = Convert.ToInt32(reader["Threshold"]),
                            Period = Convert.ToInt32(reader["Period"])
                        });
                    }
                }
            }

            return result;
        }

        public static List<HostConfigurationEntity> GetList(string host)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<HostConfigurationEntity> result = new List<HostConfigurationEntity>();
            StringBuilder query = new StringBuilder(@"SELECT [Host],
                                                             [Threshold],
                                                             [Period],
                                                             [Id] FROM [t_Host_Configuration] ");
            StringBuilder where = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(host))
            {
                where.Append(" Host LIKE'%'+@host+'%' ");
            }
            if (where.Length > 0)
            {
                query.AppendFormat(" WHERE {0}", where.ToString());
            }

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                if (!string.IsNullOrWhiteSpace(host))
                {
                    cmd.Parameters.AddWithValue("@host", host);
                }
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        HostConfigurationEntity item = new HostConfigurationEntity();
                        item.ID = index++;
                        item.Host = Convert.ToString(reader["Host"]);
                        item.Period = Convert.ToInt32(reader["Period"]);
                        item.Threshold = Convert.ToInt32(reader["Threshold"]);
                        item.TableID = Convert.ToInt32(reader["Id"]);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static void Add(HostConfigurationEntity item)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"INSERT INTO dbo.t_Host_Configuration
                                                        ( Host ,
                                                          Threshold ,
                                                          Period 
                                                        )
                                                VALUES  ( @host , 
                                                          @threshold ,
                                                          @period 
                                                        )");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@host", item.Host);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(HostConfigurationEntity item)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"UPDATE dbo.t_Host_Configuration 
                                                        SET Host=@host,
                                                            Threshold=@threshold,
                                                            Period=@period WHERE Id=@id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@host", item.Host);
                cmd.Parameters.AddWithValue("@threshold", item.Threshold);
                cmd.Parameters.AddWithValue("@period", item.Period);
                cmd.Parameters.AddWithValue("@id", item.TableID);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static HostConfigurationEntity GetHostConfiguration(int id)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            HostConfigurationEntity result = new HostConfigurationEntity();
            StringBuilder query = new StringBuilder(@"SELECT [Host],
                                                             [Threshold],
                                                             [Period],
                                                             [Id] from [t_Host_Configuration] WHERE [Id] = @id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new HostConfigurationEntity();
                        result.Host = Convert.ToString(reader["Host"]);
                        result.Period = Convert.ToInt32(reader["Period"]);
                        result.Threshold = Convert.ToInt32(reader["Threshold"]);
                        result.TableID = Convert.ToInt32(reader["Id"]);
                    }
                }
            }

            return result;
        }

        public static void Delete(int id)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"DELETE dbo.t_Host_Configuration WHERE Id=@id;";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static bool Equals(string host, int id)
        {
            string cons = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"SELECT COUNT(1)
                                    FROM dbo.t_Host_Configuration
                                    WHERE Host= @host ";
                if (id > 0)
                {
                    query += " AND Id <> @id";
                }
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@host", host);
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
