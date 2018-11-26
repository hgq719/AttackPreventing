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
        public static List<HostConfiguration> GetList()
        {
            return HostConfigurationAccess.GetList();
        }
    }
}
