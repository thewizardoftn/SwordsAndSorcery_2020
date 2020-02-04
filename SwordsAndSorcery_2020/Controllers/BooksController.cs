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
    public class BooksController : Controller
    {
        public List<Chapter_Type> thisBook { get; set; }

        public ActionResult Indomitus_Est()
        {
            BookModel model = new BookModel();
            model.partialBooks = model.BookChapters("Indomitus Est");
            thisBook = model.partialBooks;

            return View(model);
        }
        public ActionResult Indomitus_Vivat()
        {
            BookModel model = new BookModel();
            model.partialBooks = model.BookChapters("Indomitus Vivat");
            thisBook = model.partialBooks;

            return View(model);
        }

        public ActionResult Indomitus_Oriens()
        {
            BookModel model = new BookModel();
            model.partialBooks = model.BookChapters("Indomitus Oriens");
            thisBook = model.partialBooks;

            return View(model);
        }

        public ActionResult Indomitus_Sum()
        {
            BookModel model = new BookModel();
            model.partialBooks = model.BookChapters("Indomitus Sum");
            thisBook = model.partialBooks;

            return View(model);
        }
        public ActionResult Semper_Indomitus()
        {
            BookModel model = new BookModel();
            model.partialBooks = model.BookChapters("Semper Indomitus");
            thisBook = model.partialBooks;

            return View(model);
        }
        public ActionResult Intermission()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Next(string chap)
        {
            int iChap = Int16.Parse(chap);
            var model = thisBook[iChap];
            return Json(model.Chapter);
        }
    }
}