using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class HostConfigurationBusiness
    {
        public static List<HostConfigurationEntity> GetList()
        {
            return HostConfigurationAccess.GetList();
        }

        public static dynamic GetList(int limit, int offset, string host)
        {
            List<HostConfigurationEntity> list = HostConfigurationAccess.GetList(host);

            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();

            return new { total, rows };
        }

        public static void Add(HostConfigurationEntity item)
        {
            HostConfigurationAccess.Add(item);
        }

        public static void Edit(HostConfigurationEntity item)
        {
            HostConfigurationAccess.Edit(item);
        }

        public static HostConfigurationEntity GetHostConfiguration(int id)
        {
            return HostConfigurationAccess.GetHostConfiguration(id);
        }

        public static void Delete(int id)
        {
            HostConfigurationAccess.Delete(id);
        }

        public static bool Equals(string host, int id)
        {
            return HostConfigurationAccess.Equals(host, id);
        }
    }
}
