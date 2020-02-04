using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using SwordsAndSorcery_2020.Models;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020
{
    public abstract class ApplicationViewPage<T> : WebViewPage<T>
    {
        protected override void InitializePage()
        {
            SetViewBagDefaultProperties();

            base.InitializePage();
            string Page = Request.Url.AbsolutePath.Replace("/swordsandsorcerycom/", "");
            //deal with the root being a blank
            if (Page.Length == 0)
                Page = "home";
            string IP = Utils.GetIPAddress();
            
            if (!Request.Url.AbsoluteUri.Contains("localhost"))
            {
                PageHit p = new PageHit(Page, IP);
            }

            if (Page != "/Home/ErrorPage")
            {

                UserType user = UserCache.GetFromCache(0, IP);
                if (user == null)
                {
                    //this is a sign that the database is down
                    Response.Redirect(ConfigurationManager.AppSettings["SitePrefix"].StringSafe() + "Home/ErrorPage");
                }

                if (user != null)
                {
                    //UserCache.Update(user.UserID, Page, user.Confirmed);
                    ViewBag.UserID = user.UserID;
                    ViewBag.UserName = user.UserName;
                    ViewBag.IPAddress = IP;
                    ViewBag.Minimize = user.DownArrow;
                }
            }
        }

        
        private void SetViewBagDefaultProperties()
        {

            ViewBag.Developer = "Robert W. Brady, Jr.";
            ViewBag.DeveloperEmail = "mailto:bob@theguf.com?Subject=Question from web page";

            ViewBag.Css = "~/Content/Site.css?v=1";
        }
    }
}