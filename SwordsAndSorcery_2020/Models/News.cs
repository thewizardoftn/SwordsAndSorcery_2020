using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using DataTier;
using DataTier.Repository;
using DataTier.Types;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class News
    {
        private List<News_Type> _news;
        public List<News_Type> NewsList { get { return _news; } set { _news = value; } }
        public int UserID { get; set; }

        public News()
        {
            UserType user = UserCache.GetFromCache(0, HttpContext.Current.Request.UserHostAddress);
            UserID = user.UserID;
            DataTable dt = new DataTable();
            _news = new List<News_Type>();
            var News = new NewsRepository(UserID, "News", "News").GetNews();
            foreach (var item in News)
            {
                _news.Add(new News_Type
                {
                    ID = item.ID,
                    NewsDate = item.NewsDate,
                    NewsItem = item.NewsItem
                });
            }
        }
        public string InsertNews(string date, string content)
        {
            string sRet = new NewsRepository(UserID, "News", "InsertNews").InsertNews(date, content);
            
            return sRet;
        }
    }
}