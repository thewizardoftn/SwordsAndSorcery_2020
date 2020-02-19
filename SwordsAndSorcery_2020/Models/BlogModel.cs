using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;
using DataTier.Repository;
using DataTier.Types;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class BlogModel
    {
        private IEnumerable<BlogType> theBlog;
        private BlogType blogReply;
        private string respEmail = ConfigurationManager.AppSettings["EmailAddress"].StringSafe();

        public string AuthorName { get; set; }
        public DateTime BlogDate { get; set; }
        public string BlogContent { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int Rights { get; set; }
        public BlogType BlogReply
        {
            get
            {
                if (blogReply == null)
                {
                    blogReply = new BlogType
                    {
                        Archive = true,
                        Author = UserID,
                        AuthorName = UserName

                    };
                }
                return blogReply;
            }
            set { blogReply = value; }
        }
        public IEnumerable<BlogType> TheBlog
        {
            get
            {
                if (theBlog == null)
                    theBlog = GetBlog();
                return theBlog;
            }
            set { theBlog = value; }
        }
        public BlogModel()
        {
            string IP = HttpContext.Current.Request.UserHostAddress;
            UserType user = UserCache.GetFromCache(0, IP);
            UserID = user.UserID;
            UserName = user.UserName;
            Rights = user.RoleId;
            AuthorName = user.UserName;
        }
        public int AddTopic(string Title, DateTime BlogDate, int Author, string Content)
        {
            return new BlogRepository(UserID, "BlogModel", "AddTopic").AddTopic(Title, BlogDate, Content, Author);
        }
        public string BlogProp(int ID, string Func, bool Value)
        {
            return new BlogRepository(UserID, "BlogModel", "BlogProp").BlogProp(ID, Func, Value);
        }

        public List<BlogType> GetAllBlogPosts()
        {
            BlogRepository br = new BlogRepository(UserID, "BlogModel", "GetAllBlogPosts");
            br.GetWholeBlog();
            List<Blog_Type> _blog = br.TheBlog;
            List<BlogType> blog = new List<BlogType>();

            foreach (var item in _blog)
            {
                blog.Add(new BlogType
                {
                    Archive = item.Archive,
                    Author = item.Author,
                    AuthorName = item.AuthorName,
                    Banned = item.Banned,
                    BlogContent = item.BlogContent,
                    BlogDate = item.BlogDate,
                    BlogID = item.BlogID,
                    BlogTrailID = item.BlogTrailID,
                    ParentID = item.ParentID,
                    Sticky = item.Sticky,
                    Title = item.Title,
                    SubCategories = GetSubCategories(item.SubCategories)
                });
            }

            return blog;
        }
        public List<BlogType> GetBlog()
        {
            BlogRepository br = new BlogRepository(UserID, "BlogModel", "GetBlog");
            br.GetPublicBlog();
            List<Blog_Type> _blog = br.TheBlog;
            List<BlogType> blog = new List<BlogType>();

            foreach (var item in _blog)
            {
                blog.Add(new BlogType
                {
                    Archive = item.Archive,
                    Author = item.Author,
                    AuthorName = item.AuthorName,
                    Banned = item.Banned,
                    BlogContent = item.BlogContent,
                    BlogDate = item.BlogDate,
                    BlogID = item.BlogID,
                    BlogTrailID = item.BlogTrailID,
                    ParentID = item.ParentID,
                    Sticky = item.Sticky,
                    Title = item.Title,
                    SubCategories = GetSubCategories(item.SubCategories)
                });
            }

            return blog;

        }
        private List<BlogType> GetSubCategories(List<Blog_Type> _blog)
        {
            List<BlogType> blog = new List<BlogType>();
            if (_blog.Count == 0)
                return blog;

            foreach (var item in _blog)
            {
                blog.Add(new BlogType
                {
                    Archive = item.Archive,
                    Author = item.Author,
                    AuthorName = item.AuthorName,
                    Banned = item.Banned,
                    BlogContent = item.BlogContent,
                    BlogDate = item.BlogDate,
                    BlogID = item.BlogID,
                    BlogTrailID = item.BlogTrailID,
                    ParentID = item.ParentID,
                    Sticky = item.Sticky,
                    Title = item.Title,
                    SubCategories = GetSubCategories(item.SubCategories)
                });
            }

            return blog;
        }

        public int SubmitReply(int BlogID, int Author, string AuthorName, string Title, string Content)
        {
            BlogRepository br = new BlogRepository(Author, "BlogModel", "SubmitReply");
            Blog_Type type = new Blog_Type
            {
                Archive = true,
                Author = Author,
                AuthorName = AuthorName,
                Banned = false,
                BlogContent = Content,
                BlogDate = DateTime.Now,
                ParentID = BlogID,
                Sticky = false,
                Title = Title
            };
            int iRet = br.AddEntry(type);

            if (iRet > 0)
            {
                //send me an email to tell me what's going on
                UserType _user = new UserType
                {
                    Email = ConfigurationManager.AppSettings["AccountEmail"].StringSafe(),
                    UserName = "Admin"
                };
                List<UserType> recipients = new List<UserType>();
                recipients.Add(_user);
                EmailType _type = new EmailType
                {
                    Message = Content,
                    SentTime = DateTime.Now,
                    SenderName = AuthorName,
                    ID = Author
                };

                Email send = new Email();
                send.SendAlert(false, "A reply is sent on the blog", respEmail, "An error occurred on the site");
            }
            return iRet;
        }
    }
}