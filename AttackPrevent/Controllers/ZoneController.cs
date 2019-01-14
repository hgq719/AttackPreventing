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
    public class ZoneController : BaseController
    {
        // GET: Zone
        public ActionResult ZoneIndex()
        {
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        public JsonResult GetZoneList(int limit, int offset, string zoneID, string zoneName, bool ifTest, bool ifEnabel)
        {
            dynamic result = ZoneBusiness.GetList(limit, offset, zoneID, zoneName, ifTest, ifEnabel);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddZone()
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddZone(Models.ZoneModel zoneModel)
        {
            if (ModelState.IsValid)
            {
                ZoneEntity item = new ZoneEntity()
                {
                    ZoneId = zoneModel.ZoneId,
                    ZoneName = zoneModel.ZoneName,
                    AuthEmail = zoneModel.AuthEmail,
                    AuthKey = Utils.AesEncrypt(zoneModel.AuthKey),
                    IfAttacking = false,
                    IfEnable = true,
                    IfTestStage = zoneModel.IfTestStage,
                    PeriodForHost = zoneModel.PeriodForHost,
                    ThresholdForHost = zoneModel.ThresholdForHost,
                    IfAnalyzeByHostRule = zoneModel.IfAnalyzeByHostRule
                };

                if (ZoneBusiness.Equals(item.ZoneId, 0))
                {
                    ViewBag.ErrorMessage = "Zone Id already exists";
                    return View(zoneModel);
                }
                else
                {
                    ZoneBusiness.Add(item);
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = Request.UserHostAddress,
                        LogType = LogLevel.Audit,
                        ZoneID = item.ZoneId,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        Detail = $"[Audit] {"AddZone"} {JsonConvert.SerializeObject(zoneModel)}",
                    });
                    return RedirectToAction("ZoneIndex");
                }

            }
            else
            {
                return View(zoneModel);
            }
        }

        public ActionResult EditZone(int id)
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }

            ZoneEntity entity = ZoneBusiness.GetZone(id);

            Models.ZoneModel model = new Models.ZoneModel()
            {
                AuthEmail = entity.AuthEmail,
                AuthKey = Utils.AesDecrypt(entity.AuthKey),
                IfEnable = entity.IfEnable,
                IfTestStage = entity.IfTestStage,
                TableID = entity.TableID,
                ZoneId = entity.ZoneId,
                ZoneName = entity.ZoneName,
                IfAttacking = entity.IfAttacking,
                PeriodForHost = entity.PeriodForHost,
                ThresholdForHost = entity.ThresholdForHost,
                IfAnalyzeByHostRule = entity.IfAnalyzeByHostRule
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult EditZone(Models.ZoneModel zoneModel)
        {
            if (ModelState.IsValid)
            {
                ZoneEntity item = new ZoneEntity()
                {
                    ZoneId = zoneModel.ZoneId,
                    ZoneName = zoneModel.ZoneName,
                    AuthEmail = zoneModel.AuthEmail,
                    AuthKey = Utils.AesEncrypt(zoneModel.AuthKey),
                    IfAttacking = zoneModel.IfAttacking,
                    IfEnable = zoneModel.IfEnable,
                    IfTestStage = zoneModel.IfTestStage,
                    TableID = zoneModel.TableID,
                    PeriodForHost = zoneModel.PeriodForHost,
                    ThresholdForHost = zoneModel.ThresholdForHost,
                    IfAnalyzeByHostRule = zoneModel.IfAnalyzeByHostRule
                };
                if (ZoneBusiness.Equals(item.ZoneId, item.TableID))
                {
                    ViewBag.ErrorMessage = "Zone Id already exists";
                    return View(zoneModel);
                }
                else
                {
                    ZoneBusiness.Update(item);
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = Request.UserHostAddress,
                        LogType = LogLevel.Audit,
                        ZoneID = item.ZoneId,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        Detail = $"[Audit] {"EditZone"} {JsonConvert.SerializeObject(zoneModel)}",
                    });
                    return RedirectToAction("ZoneIndex");
                }
            }
            else
            {
                return View(zoneModel);
            }
        }

        public ActionResult EditZoneIfTest(int id, bool ifTest)
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            ZoneEntity entity = ZoneBusiness.GetZone(id);
            entity.IfTestStage = ifTest;
            ZoneBusiness.Update(entity);
            return RedirectToAction("ZoneIndex");
        }

        public ActionResult EditZoneIfEnable(int id, bool ifEnable)
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            ZoneEntity entity = ZoneBusiness.GetZone(id);
            entity.IfEnable = ifEnable;
            ZoneBusiness.Update(entity);
            return RedirectToAction("ZoneIndex");
        }

        public ActionResult GetZoneIfAttacking(string zone)
        {
            ViewBag.IfAttacking = ZoneBusiness.GetZone(zone, zone).IfAttacking;
            return View();
        }
    }
}