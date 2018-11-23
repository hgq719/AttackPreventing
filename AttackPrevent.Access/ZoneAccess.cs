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
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<ZoneEntity> result = new List<ZoneEntity>();
            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = "select [ZoneId],[ZoneName],[AuthEmail],[IfTestStage],[IfEnable],[IfAttacking],[AuthKey] from [t_Zone_Info] ";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ZoneEntity item = new ZoneEntity();
                        item.ZoneId = reader.GetString(0);
                        item.ZoneName = reader.GetString(1);
                        item.AuthEmail = reader.GetString(2);
                        item.IfTestStage = reader.GetInt32(3) > 0;
                        item.IfEnable = reader.GetInt32(4) > 0;
                        item.IfAttacking = reader.GetInt32(5) > 0;
                        item.AuthKey = reader.GetString(6);
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static List<ZoneEntity> GetList(string zoneID, string zoneName, bool ifTest, bool ifEnabel)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            List<ZoneEntity> result = new List<ZoneEntity>();
            StringBuilder query = new StringBuilder("select [ZoneId],[ZoneName],[AuthEmail],[IfTestStage],[IfEnable],[IfAttacking], [Id] from [t_Zone_Info] ");
            StringBuilder where = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(zoneID))
            {
                where.Append(" ZoneId like'%'+@zoneID+'%' and ");
            }
            if (!string.IsNullOrWhiteSpace(zoneName))
            {
                where.Append(" ZoneName like'%'+@zoneName+'%' and ");
            }
            if (ifTest)
            {
                where.Append(" IfTestStage = @ifTest and ");
            }
            if (ifEnabel)
            {
                where.Append(" IfEnable = @ifEnabel and ");
            }
            if (where.Length > 4)
            {
                where.Remove(where.Length - 4, 4);
                query.AppendFormat(" where {0}", where.ToString());
            }

            using (SqlConnection conn = new SqlConnection(cons))
            {
                
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                if (!string.IsNullOrWhiteSpace(zoneID))
                {
                    cmd.Parameters.AddWithValue("@zoneID", zoneID);
                }
                if (!string.IsNullOrWhiteSpace(zoneName))
                {
                    cmd.Parameters.AddWithValue("@zoneName", zoneName);
                }
                if (ifTest)
                {
                    cmd.Parameters.AddWithValue("@ifTest", ifTest);
                }
                if (ifEnabel)
                {
                    cmd.Parameters.AddWithValue("@ifEnabel", ifEnabel);
                }
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        ZoneEntity item = new ZoneEntity();
                        item.ID = index++;
                        item.ZoneId = reader.GetString(0);
                        item.ZoneName = reader.GetString(1);
                        item.AuthEmail = reader.GetString(2);
                        item.IfTestStage = reader.GetInt32(3) > 0;
                        item.IfEnable = reader.GetInt32(4) > 0;
                        item.IfAttacking = reader.GetInt32(5) > 0;
                        item.TableID = reader.GetInt32(6);
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
          IfAttacking
        )
VALUES  ( @zoneID , -- ZoneId - nvarchar(512)
          @zoneName , -- ZoneName - nvarchar(256)
          @authEmail , -- AuthEmail - nvarchar(256)
          @authKey , -- AuthKey - nvarchar(256)
          @ifTest , -- IfTestStage - int
          @ifEnable , -- IfEnable - int
          @ifAttacking  -- IfAttacking - int
        )");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@zoneName", item.ZoneId);
                cmd.Parameters.AddWithValue("@authEmail", item.ZoneId);
                cmd.Parameters.AddWithValue("@authKey", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifTest", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifEnable", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifAttacking", item.ZoneId);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static void Edit(ZoneEntity item)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            StringBuilder query = new StringBuilder(@"UPDATE dbo.t_Zone_Info SET ZoneId=@zoneID,ZoneName=@zoneName,AuthEmail=@authEmail,AuthKey=@authKey,IfTestStage=@ifTest,IfEnable=@ifEnable,IfAttacking=@ifAttacking WHERE Id=@id");

            using (SqlConnection conn = new SqlConnection(cons))
            {

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@zoneID", item.ZoneId);
                cmd.Parameters.AddWithValue("@zoneName", item.ZoneId);
                cmd.Parameters.AddWithValue("@authEmail", item.ZoneId);
                cmd.Parameters.AddWithValue("@authKey", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifTest", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifEnable", item.ZoneId);
                cmd.Parameters.AddWithValue("@ifAttacking", item.ZoneId);
                cmd.Parameters.AddWithValue("@id", item.TableID);
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public static ZoneEntity GetZone(int id)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            ZoneEntity result = new ZoneEntity();
            StringBuilder query = new StringBuilder("select [ZoneId],[ZoneName],[AuthEmail],[IfTestStage],[IfEnable],[IfAttacking], [Id], [AuthKey] from [t_Zone_Info] where [Id] = @id");

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
                        result.ZoneId = reader.GetString(0);
                        result.ZoneName = reader.GetString(1);
                        result.AuthEmail = reader.GetString(2);
                        result.IfTestStage = reader.GetInt32(3) > 0;
                        result.IfEnable = reader.GetInt32(4) > 0;
                        result.IfAttacking = reader.GetInt32(5) > 0;
                        result.TableID = reader.GetInt32(6);
                        result.AuthKey = reader.GetString(7);
                    }
                }
            }

            return result;
        }
    }
}
