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
#if DEBUG
                return "DESKTOP - KIMCDIR\\PC".Split('\\').LastOrDefault();
#else
                return User.Identity.Name.Split('\\').LastOrDefault();                
#endif


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

#if DEBUG
                return true;
#else
                return adminUserList.Exists(a => a.Name.ToString().ToLower() == UserName.ToLower());
                
#endif
            }
        }
        public JsonResult CheckCloundflareAuth(string zoneID)
        {
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;

            bool isSuccessed = false;
            string errorMsg = "";

            ICloundFlareApiService cloundFlareApiService = new CloundFlareApiService();
            string strResult = cloundFlareApiService.CheckAuth(zoneID, authEmail, authKey);
            if (!string.IsNullOrEmpty(strResult))
            {
                errorMsg = string.Format("Cloundflare {0}", strResult) ;
            }
            else
            {
                isSuccessed = true;
            }

            return Json(new { isSuccessed, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }
    }
}