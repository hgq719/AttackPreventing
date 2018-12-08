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
            ViewBag.ErrorMessage = string.Empty;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(Models.LoginModel loginModel)
        {
            /*
                
            */
            PrincipalContext pc = new PrincipalContext(ContextType.Machine);
            bool isCredentialValid = pc.ValidateCredentials(loginModel.UserName, loginModel.Password);
            if (isCredentialValid)
            {
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
                ViewBag.ErrorMessage = "Account or password is wrong.";
                return View(model);
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