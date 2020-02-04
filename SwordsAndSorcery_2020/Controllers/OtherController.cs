using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using SwordsAndSorcery_2020.Models;

namespace SwordsAndSorcery_2020.Controllers
{
    public class OtherController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";
            return View();
        }
        [HttpPost]
        public ActionResult FiftyReasons(HttpPostedFileBase fileLocation, Email model)
        {
            if (ModelState.IsValid)
            {
                model.FromAddress = ConfigurationManager.AppSettings["SendFrom"];
                model.Subject = "Request for the Fifty Reasons";
                model.Message = "As promised, this is your FREE .pdf of 'The Fifty Reasons'!";
                model.AttachedFile = model.GetFile(Server.MapPath("~\\Content\\The Fifty Reasons Book.pdf"));
                model.FileName = "The Fifty Reasons.pdf";
                bool bRet = model.SendEmail(true);
                if (bRet)
                {
                    ViewBag.Message = "A copy has been sent to your email";
                }
                else
                    ViewBag.Message = "An error occurred - while we repair it, try emailing directly to bob@theguf.com";

            }
            return View();
        }
    }
}