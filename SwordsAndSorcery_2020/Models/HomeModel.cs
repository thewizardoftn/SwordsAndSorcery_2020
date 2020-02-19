using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities;
using DataTier.Repository;
using SwordsAndSorcery_2020.ModelTypes;

namespace SwordsAndSorcery_2020.Models
{
    public class HomeModel
    {
        public int UserID { get; set; }
        public HomeModel()
        {
            UserType user = UserCache.GetFromCache(0, HttpContext.Current.Request.UserHostAddress);
            UserID = user.UserID;
        }
        public string SaveEMail(string email, string name)
        {
            //first, send an email
            Email em = new Email();
            em.SendAlert(false, "Request for book", email, name + " has requested a book.  His email is: " + email);

            return new EmailRepository(UserID, "HomeModel", "SaveEmail").SaveEmail(email, name);
        }
    }
}