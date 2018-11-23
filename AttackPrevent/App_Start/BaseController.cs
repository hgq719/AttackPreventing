using AttackPrevent.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent
{
    public class BaseController : Controller
    {
        public string UserName
        {
            get
            {
                return User.Identity.Name.Split('\\').LastOrDefault();
                //return "DESKTOP - KIMCDIR\\PC".Split('\\').LastOrDefault();
            }
        }
        public virtual bool IsAdmin
        {
            get
            {
                string key = "adminUserList";
                List<dynamic> adminUserList = Utils.GetMemoryCache(key, () =>
                {
                    return UserBusiness.GetUserList();
                }, 5);

                return adminUserList.Exists(a => a.Name.ToString().ToLower() == UserName.ToLower());

                //return true;
            }
        }
    }
}