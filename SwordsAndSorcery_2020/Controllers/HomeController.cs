using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;
using SwordsAndSorcery_2020.Models;

namespace SwordsAndSorcery_2020.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HomeModel model = new HomeModel();

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            var model = new FeedbackModel();

            return View(model);
        }

        public ActionResult ErrorPage()
        {

            return View();

        }

        [HttpPost]
        public ActionResult Contact(FeedbackModel model)
        {
            //verify that this isn't a robot
            if (model.Value2 + model.Value1 != model.Solution)
            {
                ModelState.AddModelError("", "Sorry, but " + model.Value1.ToString() + " + " + model.Value2.ToString() +
                    " does not equal " + model.Solution.ToString());
                return View(model);
            }
            else if (model.SenderEmail == null || model.SenderName == null)
            {
                ModelState.AddModelError("", "Name and Email Address are both required");
                return View(model);
            }
            else
            {
                bool res = model.SaveFeedback(Utils.GetIPAddress());
                if (res)
                {
                    Email e = new Email();
                    e.FromAddress = model.SenderEmail;
                    e.Message = model.Message;
                    e.SenderName = model.SenderName;
                    e.Subject = "Feedback from Swords and Sorcery";
                    e.SendEmail(false);
                    e.IP = Utils.GetIPAddress();
                    return View("Index");
                }
                else
                {
                    ModelState.AddModelError("", model.Error);

                    return View(model);
                }
            }
        }
        public ActionResult Fovea()
        {
            FoveaModel model = new FoveaModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult NewTerm(FoveaModel model)
        {
            if (model.Value2 + model.Value1 != model.Solution)
            {
                ModelState.AddModelError("", "Sorry, but " + model.Value1.ToString() + " + " + model.Value2.ToString() +
                    " does not equal " + model.Solution.ToString());
                return View("Fovea", model);
            }
            else
            {
                bool bRet = model.AddTerm();
                if (bRet)
                {
                    model = new FoveaModel();
                    model.Message = "Your update will be added when it is approved";
                }
                return View("Fovea", model);
            }
        }
        public ActionResult News()
        {
            var model = new News();
            return View(model);
        }
        public ActionResult Contests()
        {
            return View();
        }
        public ActionResult Indomitus_Est()
        {
            return View();
        }
    }
}