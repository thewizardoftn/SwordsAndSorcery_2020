using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SwordsAndSorcery_2020.Models;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Controllers
{
    public class BlogController : Controller
    {
        public ActionResult Index()
        {
            BlogModel model = new BlogModel();
            return View(model);
        }
        [HttpPost]
        public bool AppendReply(int formID)
        {
            return false;
        }

        [HttpPost]
        public JsonResult SubmitPost(string BlogID, string Author, string AuthorName, string Title, string Content)
        {
            BlogModel model = new BlogModel();
            int blogID = (int)Utils.ParseNumControlledReturn(BlogID);
            int author = (int)Utils.ParseNumControlledReturn(Author);
            int iRet = model.SubmitReply(blogID, author, AuthorName, Title, Content);

            return Json("true", JsonRequestBehavior.AllowGet);
        }
        public ActionResult StartIssue(BlogModel model)
        {
            BlogModel newModel = new BlogModel();


            if (model.Title.StringSafe().Length == 0 || model.BlogContent.StringSafe().Length == 0)
            {
                newModel.Message = "You must include an issue and a Main Item";
            }
            else
            {
                int iRet = model.AddTopic(model.Title, model.BlogDate, model.UserID, model.BlogContent);

                if (iRet < 1)
                    newModel.Message = "The program did not create a new record - a report has been generated";
                else
                {
                    newModel.Message = "";
                }
            }


            return View("Index", newModel);
        }
    }
}