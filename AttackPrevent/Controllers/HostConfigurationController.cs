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
    public class HostConfigurationController : BaseController
    {
        // GET: HostConfiguration
        public ActionResult Index()
        {
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        public JsonResult GetList(int limit, int offset, string host)
        {
            dynamic result = HostConfigurationBusiness.GetList(limit, offset, host);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: HostConfiguration/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HostConfiguration/Create
        public ActionResult Create()
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        // POST: HostConfiguration/Create
        [HttpPost]
        public ActionResult Create(Models.HostConfigurationModel hostConfigurationModel)
        {
            if (ModelState.IsValid)
            {
                HostConfigurationEntity item = new HostConfigurationEntity()
                {
                    Host = hostConfigurationModel.Host,
                    Period = hostConfigurationModel.Period,
                    Threshold = hostConfigurationModel.Threshold
                };

                if (HostConfigurationBusiness.Equals(item.Host, 0))
                {
                    ViewBag.ErrorMessage = "Host already exists";
                    return View(hostConfigurationModel);
                }
                else
                {
                    HostConfigurationBusiness.Add(item);
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = Request.UserHostAddress,
                        LogType = LogLevel.Audit.ToString(),
                        ZoneID = string.Empty,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        Detail = $"[Audit] {"AddHostConfiguration"} {JsonConvert.SerializeObject(hostConfigurationModel)}",
                    });
                    return RedirectToAction("Index");
                }
                
            }
            else
            {
                return View(hostConfigurationModel);
            }
        }

        // GET: HostConfiguration/Edit/5
        public ActionResult Edit(int id)
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            HostConfigurationEntity item = HostConfigurationBusiness.GetHostConfiguration(id);
            Models.HostConfigurationModel hostConfigurationModel = new Models.HostConfigurationModel
            {
                Host = item.Host,
                Period = item.Period,
                Threshold = item.Threshold,
                TableID = item.TableID
            };
            return View(hostConfigurationModel);
        }

        // POST: HostConfiguration/Edit/5
        [HttpPost]
        public ActionResult Edit(Models.HostConfigurationModel hostConfigurationModel)
        {
            if (ModelState.IsValid)
            {
                HostConfigurationEntity item = new HostConfigurationEntity()
                {
                    Host = hostConfigurationModel.Host,
                    Period = hostConfigurationModel.Period,
                    Threshold = hostConfigurationModel.Threshold,
                    TableID = hostConfigurationModel.TableID
                };

                if (HostConfigurationBusiness.Equals(item.Host, item.TableID))
                {
                    ViewBag.ErrorMessage = "Host already exists";
                    return View(hostConfigurationModel);
                }
                else
                {
                    HostConfigurationBusiness.Edit(item);
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = Request.UserHostAddress,
                        LogType = LogLevel.Audit.ToString(),
                        ZoneID = string.Empty,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        Detail = $"[Audit] {"EditHostConfiguration"} {JsonConvert.SerializeObject(hostConfigurationModel)}",
                    });
                }
                
            }
            else
            {
                return View(hostConfigurationModel);
            }

            return RedirectToAction("Index");
        }

        // GET: HostConfiguration/Delete/5
        public ActionResult Delete(int id)
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            HostConfigurationBusiness.Delete(id);
            return RedirectToAction("Index");
        }
        
    }
}
