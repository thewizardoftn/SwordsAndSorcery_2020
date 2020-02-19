using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTier.Repository;
using DataTier.Types;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class AdminModel
    {
        private IEnumerable<BlogType> blogFiles;

        public List<EmailType> EMails { get; set; }
        public List<Error_Type> Errors { get; set; }
        public List<IPType> IPS { get; set; }
        public List<PageHitsType> Hits { get; set; }
        public Dictionary<string, string>Requests { get; set; }
        public List<UserType> Users { get; set; }
        //News section
        public string NewsDate { get; set; }
        public string NewsItem { get; set; }
        public List<FoveaTermsType> Terms { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public int UserID {get; set; }

        public IEnumerable<BlogType> BlogFiles
        {
            set { blogFiles = value; }
            get
            {
                if (blogFiles == null)
                    blogFiles = new BlogModel().GetAllBlogPosts();
                return blogFiles;
            }
        }

        public AdminModel()
        {
            UserType user = UserCache.GetFromCache(0, HttpContext.Current.Request.UserHostAddress);
            UserID = user.UserID;

            AdminRepository a = new AdminRepository(UserID, "AdminModel", "AdminModel");
            a.GetCollections();
            Requests = a.Requests;

            List<EmailType> emails = new List<EmailType>();
            foreach (var item in a.Emails)
            {
                emails.Add(new EmailType
                {
                    ID = item.ID,
                    Message = item.Message,
                    SenderEmail = item.SenderEmail,
                    SenderName = item.SenderName,
                    SentTime = item.SentTime,
                    Subject = item.Subject

                });
            }
            EMails = emails;

            List<Error_Type> errors = new List<Error_Type>();

            foreach (var item in a.ErrorList)
            {
                errors.Add(new Error_Type
                {
                    Corrected = item.Corrected,
                    Err_Message = item.Err_Message,
                    Err_Subject = item.Err_Subject,
                    ErrorID = item.ErrorID,
                    LiteralDesc = item.LiteralDesc,
                    Page = item.Page,
                    Process = item.Process,
                    SQL = item.SQL,
                    Time_Of_Error = item.Time_Of_Error,
                    UserName = item.UserName
                });
            }
            Errors = errors;

            IPS = new List<IPType>();
            foreach (var item in a.IPS)
            {
                IPS.Add(new IPType
                {
                    Hits = item.Hits,
                    IP = item.IP
                });
            }

            Hits = new List<PageHitsType>();
            foreach (var item in a.Hits)
            {
                Hits.Add(new PageHitsType
                {
                    HitDate = item.HitDate,
                    Hits = item.Hits,
                    PageName = item.PageName
                });
            }
            Users = new List<UserType>();
            foreach (var item in a.Users)
            {
                Users.Add(new UserType
                {
                    CreateDate = item.CreateDate,
                    Confirmed = item.IsConfirmed,
                    UserID = item.UserID,
                    UserName = item.UserName
                });
            }
            List<FoveaTerms_Type> terms = a.Terms;
            List<FoveaTermsType> _terms = new List<FoveaTermsType>();

            if(terms != null)
                foreach (var item in terms)
                {
                    _terms.Add(new FoveaTermsType
                    {
                        Approved = item.Approved,
                        Description = item.Description,
                        FoveaID = item.FoveaID,
                        Subject = item.Subject
                    });
                }
            Terms = _terms;
        }
        public string NewNews(string date, string news)
        {
            return new News().InsertNews(date, news);
        }
        public bool AddTerm()
        {
            bool bRet = new FoveanTerms().AddApprovedTerm(Subject, Description);
            return bRet;
        }
        public bool UpdateTerm(int FoveaID, string Subject, string Description)
        {
            return new FoveanTerms().UpdateTerm(Subject, Description, FoveaID);
        }
        public bool RemoveTerm(int FoveaID)
        {
            bool bRet = new FoveanTerms().RemoveTerm(FoveaID);

            return bRet;
        }
        public string PeopleOn
        {
            get
            {

                int _count = UserCache.Count();
                string mess = "<span>There are " + _count + " people on the site right now</span>";
                return mess;
            }
        }

        public Dictionary<string, string> UserList
        {
            get
            {
                return UserCache.Output();

            }
        }
    }
}