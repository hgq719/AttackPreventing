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
        //Code Review by michael, 程序出错了怎么办，又没有记录日志.
        public static List<GlobalConfiguration> GetConfigurationList()
        {
            return GlobalConfigurationAccess.GetList();
        }
    }
}
