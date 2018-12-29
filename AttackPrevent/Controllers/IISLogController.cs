using AttackPrevent.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class IISLogController : BaseController
    {
        public ActionResult IISLogs()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectListForAuditLog();
            Utils.RemoveMemoryCache(IISLogBusiness.cacheKey + UserName);
            return View();
        }

        public JsonResult GetIISLog(int limit, int offset, int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail, bool ifUseCache)
        {
            //dynamic result = AuditLogBusiness.GetAuditLog(limit, offset, zoneTableID, startTime, endTime, logType, detail, ifUseCache, UserName);
            dynamic result = IISLogBusiness.GetAuditLogByPage(limit, offset, zoneTableID, startTime, endTime, logType, detail);
            return new JsonResult()
            {
                Data = result,
                MaxJsonLength = Int32.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportIISLog(int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            MemoryStream ms = IISLogBusiness.ExportAuditLog(zoneTableID, startTime, endTime, logType, detail);
            return File(ms, "application/vnd.ms-excel", "IISLog.xls");
        }
    }
}