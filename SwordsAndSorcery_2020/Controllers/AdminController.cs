using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;
using SwordsAndSorcery_2020.Models;
using SwordsAndSorcery_2020.ModelTypes;

namespace SwordsAndSorcery_2020.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if(ViewBag.IPAddress == null)
            {
                ViewBag.IPAddress = HttpContext.Request.UserHostAddress;
            }
            UserType user = UserCache.findInCache(ViewBag.IPAddress);
            if (user == null)
                Response.Redirect("Home");
            else if (user.UserID != 2)
                Response.Redirect("Home");

            AdminModel a = new AdminModel();
            return View(a);
        }
        [HttpPost]
        public string PostNews(string date, string item)
        {
            return new AdminModel().NewNews(date, item);
        }
        [HttpPost]
        public ActionResult NewTerm(AdminModel model)
        {
            bool bRet = model.AddTerm();

            return View("Index", model);
        }
        public ActionResult PostUpdate(AdminModel model)
        {
            foreach (var item in model.Terms)
            {
                if (item.Approved)
                    model.UpdateTerm(item.FoveaID, item.Subject, item.Description);
                else
                    model.RemoveTerm(item.FoveaID);
            }
            model.Terms = new List<FoveaTermsType>();
            return View("Index", model);
        }
        [HttpPost]
        public JsonResult DeleteEmail(int emailID)
        {
            Email e = new Email();
            bool bRet = e.DeleteEmail(emailID);
            if (bRet)
                return Json("Success", JsonRequestBehavior.AllowGet);
            else
                return Json("Delete Failed!", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DeleteFovean(string TermID)
        {
            FoveanTerms f = new FoveanTerms();
            int iTermID = (int)Utils.ParseNumControlledReturn(TermID);

            bool bRet = f.RemoveTerm(iTermID);
            if (bRet)
                return Json("Success", JsonRequestBehavior.AllowGet);
            else
                return Json("Delete Failed!", JsonRequestBehavior.AllowGet);
        }
    }
}