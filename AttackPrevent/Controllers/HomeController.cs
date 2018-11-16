using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class HomeController : Controller
    {
        //
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CloundflareDownloadLogs()
        {
            return View();
        }

        public ActionResult WhiteList()
        {
            return View();
        }

        public ActionResult BlackList()
        {
            return View();
        }

        public ActionResult RateLimitingList()
        {
            return View();
        }

        public ActionResult AuditLogs() 
        {
            return View();
        }

        public ActionResult AddWhiteList()
        {
            return View();
        }

        public ActionResult AddBlackList()
        {
            return View();
        }

        public ActionResult AddAndEditRateLimiting()
        {
            return View();
        }
    }
}