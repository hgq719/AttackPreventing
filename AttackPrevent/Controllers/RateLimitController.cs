using AttackPrevent.Business;
using AttackPrevent.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class RateLimitController : BaseController
    {
        // GET: RateLimit
        public ActionResult RateLimitIndex(string zoneId = "")
        {
            var ZoneList = ZoneBusiness.GetZoneSelectList();
            ViewBag.ZoneList = ZoneList;
            ViewBag.DefaultValue = zoneId;
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        public JsonResult RateLimitGetMaxOrder(string zoneId)
        {
            return Json(new { maxOrderNo = RateLimitBusiness.GetRateLimitMaxOrder(zoneId) }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRateLimiting(int limit, int offset, string zoneID, DateTime? startTime, DateTime? endTime, string url)
        {
            dynamic result = RateLimitBusiness.GetRateLimit(limit, offset, zoneID, startTime, endTime, url);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRateLimiting()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddRateLimiting(Models.RateLimitModel rateLimitModel)
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (ModelState.IsValid)
            {
                RateLimitEntity item = new RateLimitEntity()
                {
                    ZoneId = rateLimitModel.ZoneId,
                    Period = rateLimitModel.Period,
                    EnlargementFactor = rateLimitModel.EnlargementFactor,
                    RateLimitTriggerIpCount = rateLimitModel.RateLimitTriggerIpCount,
                    RateLimitTriggerTime = rateLimitModel.RateLimitTriggerTime,
                    Threshold = rateLimitModel.Threshold,
                    Url = rateLimitModel.Url,
                    CreatedBy = UserName
                };

                RateLimitBusiness.Add(item);
                AuditLogBusiness.Add(new AuditLogEntity
                {
                    IP = Request.UserHostAddress,
                    LogType = LogLevel.Audit,
                    ZoneID = rateLimitModel.ZoneId,
                    LogOperator = UserName,
                    LogTime = DateTime.UtcNow,
                    Detail = $"[Audit] {"AddRateLimit"} {JsonConvert.SerializeObject(rateLimitModel)}",
                });
                return RedirectToAction("RateLimitIndex", new { zoneId = rateLimitModel.ZoneId });
            }
            else
            {
                return View(rateLimitModel);
            }
        }

        public ActionResult EditRateLimiting(int id)
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }

            RateLimitEntity item = RateLimitBusiness.GetRateLimitByID(id);

            Models.RateLimitModel rateLimitModel = new Models.RateLimitModel()
            {
                EnlargementFactor = item.EnlargementFactor,
                Period = item.Period,
                RateLimitTriggerIpCount = item.RateLimitTriggerIpCount,
                RateLimitTriggerTime = item.RateLimitTriggerTime,
                Threshold = item.Threshold,
                Url = item.Url,
                ZoneId = item.ZoneId,
                TableID = item.TableID
            };
            return View(rateLimitModel);
        }

        [HttpPost]
        public ActionResult EditRateLimiting(Models.RateLimitModel rateLimitModel)
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (ModelState.IsValid)
            {
                RateLimitEntity item = new RateLimitEntity()
                {
                    CreatedBy = UserName,
                    EnlargementFactor = rateLimitModel.EnlargementFactor,
                    Period = rateLimitModel.Period,
                    ZoneId = rateLimitModel.ZoneId,
                    RateLimitTriggerIpCount = rateLimitModel.RateLimitTriggerIpCount,
                    RateLimitTriggerTime = rateLimitModel.RateLimitTriggerTime,
                    TableID = rateLimitModel.TableID,
                    Threshold = rateLimitModel.Threshold,
                    Url = rateLimitModel.Url
                };

                RateLimitBusiness.Update(item);
                AuditLogBusiness.Add(new AuditLogEntity
                {
                    IP = Request.UserHostAddress,
                    LogType = LogLevel.Audit,
                    ZoneID = rateLimitModel.ZoneId,
                    LogOperator = UserName,
                    LogTime = DateTime.UtcNow,
                    Detail = $"[Audit] {"EditRateLimit"} {JsonConvert.SerializeObject(rateLimitModel)}",
                });
            }
            else
            {
                return View(rateLimitModel);
            }

            return RedirectToAction("RateLimitIndex", new { zoneId = rateLimitModel.ZoneId });
        }

        public ActionResult DeleteRateLimiting(int id, int order)
        {
            RateLimitEntity item = RateLimitBusiness.GetRateLimitByID(id);
            RateLimitBusiness.Delete(id, order, item.ZoneId);

            AuditLogBusiness.Add(new AuditLogEntity
            {
                IP = Request.UserHostAddress,
                LogType = LogLevel.Audit,
                ZoneID = item.ZoneId,
                LogOperator = UserName,
                LogTime = DateTime.UtcNow,
                Detail = $"[Audit] {"DeleteRateLimit"} {JsonConvert.SerializeObject(item)}",
            });
            return RedirectToAction("RateLimitIndex", new { zoneId = item.ZoneId });
        }

        public ActionResult EditRateLimitingOrder(int id, int order, int actionb, string zoneId)
        {
            RateLimitBusiness.UpdateOrder(actionb, id, order, zoneId);
            RateLimitEntity item = RateLimitBusiness.GetRateLimitByID(id);
            string optionStr = actionb == 1 ? "up" : "down";
            AuditLogBusiness.Add(new AuditLogEntity
            {
                IP = Request.UserHostAddress,
                LogType = LogLevel.Audit,
                ZoneID = item.ZoneId,
                LogOperator = UserName,
                LogTime = DateTime.UtcNow,
                Detail = $"[Audit] {"EditRateLimit order"} [{optionStr}] {JsonConvert.SerializeObject(item)}",
            });
            return RedirectToAction("RateLimitIndex", new { zoneId });
        }
    }
}