using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTier.Repository;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class PageHit
    {
        public PageHit(string page, string ip)
        {
            UserType user = UserCache.GetFromCache(0, HttpContext.Current.Request.UserHostAddress);
            if (user == null)
                return;   

            PageHits p = new PageHits(page, ip, user.UserID, "PageHit");
            if (!p.NeedUpdate())
                p.DoUpdate();
        }
    }
}