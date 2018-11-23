using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class GlobalConfigurationBusiness
    {
        public static List<GlobalConfiguration> GetConfigurationList()
        {
            return GlobalConfigurationAccess.GetList();
        }
    }
}
