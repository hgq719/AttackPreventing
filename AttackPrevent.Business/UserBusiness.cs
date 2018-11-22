using AttackPrevent.Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class UserBusiness
    {
        public static List<dynamic> GetUserList()
        {
            return UserAccess.GetList();
        }
    }
}
