using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AttackPrevent.Business
{
    public class ZoneBusiness
    {
        public static List<ZoneEntity> GetAllList()
        {
            return ZoneAccess.GetAllList();
        }

        public static List<SelectListItem> GetZoneSelectList()
        {
            //Code review By Michael 不要的代码就都删掉吧.
            //List<SelectListItem> zonelist = new List<SelectListItem>() {
            //    new SelectListItem() {Value="111",Text="ent.comm100.com"},
            //    new SelectListItem() {Value="222",Text="hosted.comm100.com"},
            //    new SelectListItem() {Value="333",Text="app.comm100.com"},
            //};

            List<SelectListItem> zonelist = new List<SelectListItem>();
            List<ZoneEntity> alllist = GetAllList();

            alllist.ForEach(item =>
            {
                zonelist.Add(new SelectListItem() { Value = item.ZoneId, Text = item.ZoneName });
            });


            return zonelist;
        }

        public static List<ZoneEntity> GetZoneList()
        {
            /*Code review by michael, 感觉这样就可以了.
            return Utils.GetMemoryCache("GetZoneList", () =>
            {
                return GetAllList();
            }, 5);*/

            string key = "GetZoneList";
            return Utils.GetMemoryCache(key, () =>
            {
                return GetAllList();
            }, 5);
        }

        public static dynamic GetList(int limit, int offset, string zoneID, string zoneName, bool ifTest, bool ifEnabel)
        {
            List<ZoneEntity> list = ZoneAccess.GetList(zoneID, zoneName, ifTest, ifEnabel);

            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();

            return new { total, rows };
        }

        public static void Add(ZoneEntity item)
        {

            ZoneAccess.Add(item);

        }

        public static void Update(ZoneEntity item)
        {
            ZoneAccess.Edit(item);

        }

        public static ZoneEntity GetZone(int id)
        {
            return ZoneAccess.GetZone(id);
        }

        public static ZoneEntity GetZone(string zoneID, string zoneName)
        {
            return ZoneAccess.GetZone(zoneID, zoneName);
        }

        public static bool UpdateAttackFlag(bool ifAttacking, string zoneId)
        {
            return ZoneAccess.UpdateAttackFlag(ifAttacking, zoneId);
        }

        public static bool Equals(string zoneId, int id)
        {
            return ZoneAccess.Equals(zoneId, id);
        }

        public static bool AuthenticateUser(string username, string password)
        {
            string ldap_path = "WinNT://" + Environment.MachineName;
            //string ldap_path = "LDAP://127.0.0.1";

            //DirectoryEntry machine = new DirectoryEntry(ldap_path);
            ////获得计算机实例
            //DirectoryEntry user = machine.Children.Find(username, "User");
            //var list = user.Properties;
            //foreach (PropertyValueCollection item in list)
            //{
            //    Console.WriteLine(item.PropertyName + ":" + item.Value.ToString());
            //}
            //return true;

            DirectoryEntry _entry = new DirectoryEntry(ldap_path, username, password);

            bool _authenticated = false;
            try
            {
                Object _o = _entry.NativeObject;
                _authenticated = true;
            }
            catch
            {
                _authenticated = false;
            }
            finally
            {
                // Avoids the "multiple connections to server not allowed" error.
                _entry.Close();
                _entry.Dispose();
            }

            return _authenticated;
        }

    }
}
