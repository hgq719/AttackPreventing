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

        public static bool UpdateAttackFlag(bool ifAttacking, string zoneId)
        {
            string cons = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(cons))
            {
                string query = @"UPDATE T_ZONE_INFO SET IfAttacking = @IfAttacking where ZoneId = @ZoneId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ZoneId", zoneId);
                cmd.Parameters.AddWithValue("@IfAttacking", ifAttacking ? 1 : 0);
                conn.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
