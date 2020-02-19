using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;
using DataTier.Repository;
using DataTier.Types;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public partial class BookModel
    {
        public List<Chapter_Type> partialBooks { get; set; }

        private int i = 0;
        public int Index { get { return i; } set { value = i; } }
        public int UserID { get; set; }

        public BookModel()
        {
            UserType user = UserCache.GetFromCache(0, HttpContext.Current.Request.UserHostAddress);
            UserID = user.UserID;
        }

        public void Next()
        {
            Index += 1;
        }

        public List<Chapter_Type> BookChapters(string BookName)
        {
            List<ChapterType> _books = new ChapterRepository(BookName, UserID, "BookModel", "BookChapters").Books;
            List<Chapter_Type> books = new List<Chapter_Type>();
            foreach (var item in _books)
            {
                books.Add(new Chapter_Type
                {
                    BookName = item.BookName,
                    Chapter = item.Chapter,
                    ChapterName = item.ChapterName,
                    DisplayOrder = item.DisplayOrder,
                    Title = item.Title
                });
            }
            return books;
        }
    }
}