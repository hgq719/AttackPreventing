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
                return User.Identity.Name;
            }
        }
        public bool IsAdmin
        {
            get
            {
                //string key = "adminUserList";
                //List<dynamic> adminUserList = Utils.GetMemoryCache(key, () =>
                //{
                //    return UserBusiness.GetUserList();
                //});

                //return adminUserList.Exists(a => a.Name == UserName.ToLower());

                return true;
            }
        }
    }
}