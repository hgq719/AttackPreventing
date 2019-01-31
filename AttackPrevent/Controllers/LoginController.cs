using AttackPrevent.Business;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index(string ReturnUrl)
        {
            if (Session["UserName"] != null)
            {
                return RedirectToAction("CloundflareDownloadLogs", "Home");
            }
            Models.LoginModel model = new Models.LoginModel
            {
                ReturnUrl = ReturnUrl

            };
            ViewBag.ErrorTimes = 0;
            ViewBag.ErrorMessage = string.Empty;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Models.LoginModel loginModel)
        {
            
            if (ModelState.IsValid)
            {
                int errorTimes = string.IsNullOrWhiteSpace(CookieHelper.GetCookie(loginModel.UserName + "errorTimes")) ? 0 : Convert.ToInt32(CookieHelper.GetCookie(loginModel.UserName+ "errorTimes"));
                PrincipalContext pc = new PrincipalContext(ContextType.Machine);
                bool isCredentialValid = pc.ValidateCredentials(loginModel.UserName, loginModel.Password);

                string checkCode = Session["CheckCode"] == null ? "" : Session["CheckCode"].ToString();
                bool verificationCheck = !(errorTimes >= 10 && !loginModel.verificationcode.Equals(checkCode));
                if (isCredentialValid)
                {
                    CookieHelper.SetCookie(loginModel.UserName + "errorTimes", "0", DateTime.UtcNow.AddDays(-1));
                    Session["UserName"] = loginModel.UserName;
                    if (Url.IsLocalUrl(loginModel.ReturnUrl) && loginModel.ReturnUrl.Length > 1 && loginModel.ReturnUrl.StartsWith("/") && !loginModel.ReturnUrl.StartsWith("//") && !loginModel.ReturnUrl.StartsWith("/\\"))
                    {
                        return Redirect(loginModel.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("CloundflareDownloadLogs", "Home");
                    }
                }
                else
                {
                    Models.LoginModel model = new Models.LoginModel
                    {
                        ReturnUrl = loginModel.ReturnUrl,
                        UserName = loginModel.UserName

                    };
                    //errorTimes += 1;
                    CookieHelper.SetCookie(loginModel.UserName + "errorTimes", errorTimes.ToString(), DateTime.UtcNow.AddMinutes(3));
                    ViewBag.ErrorTimes = errorTimes;
                    ViewBag.ErrorMessage = "Account or password is wrong.";
                    return View(model);
                }
            }
            else
            {
                ViewBag.ErrorTimes = 0;
                return View(loginModel);
            }
            
        }

        // POST: /Account/LogOff
        //[HttpPost]
        public ActionResult LogOff()
        {
            Session["UserName"] = null;
            return RedirectToAction("Index", "Login");
        }
    }
}