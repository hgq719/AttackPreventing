﻿using AttackPrevent.Business;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace AttackPrevent.Controllers
{
    public class AuditLogController : BaseController
    {
        public ActionResult AuditLogs()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectListForAuditLog();
            Utils.RemoveMemoryCache(AuditLogBusiness.cacheKey + UserName);
            return View();
        }

        public JsonResult GetAuditLog(int limit, int offset, int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail, bool ifUseCache)
        {
            //dynamic result = AuditLogBusiness.GetAuditLog(limit, offset, zoneTableID, startTime, endTime, logType, detail, ifUseCache, UserName);
            dynamic result = AuditLogBusiness.GetAuditLogByPage(limit, offset, zoneTableID, startTime, endTime, logType, detail);
            return new JsonResult()
            {
                Data = result,
                MaxJsonLength = Int32.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportAuditLog(int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            MemoryStream ms = AuditLogBusiness.ExportAuditLog(zoneTableID, startTime, endTime, logType, detail);
            return File(ms, "application/vnd.ms-excel", "AuditLog.xls");
        }
    }
}