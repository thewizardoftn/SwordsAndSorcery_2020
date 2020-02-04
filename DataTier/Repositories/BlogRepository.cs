using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class BlogRepository
    {
        public int UserID { get; set; }
        public List<Blog_Type> TheBlog { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }

        public BlogRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
        }

        public int AddTopic(string Title, DateTime BlogDate, string Content, int AuthorID)
        {
            string sSQL = "INSERT INTO [dbo].[Blog] ([Title], [BlogDate], [Author], [BlogContent], Parent, Archive) " +
                    "VALUES(@Title, getdate(), @Author, @BlogContent, 0, 0); SELECT Scope_Identity();";
            int iRet = -1;
            using (Data DC = new Data("conn", Page, Process))
            {

                DC.AddCommand(CommandType.Text, sSQL);

                try
                {
                    DC.AttachParameterByValue("Title", Title);
                    DC.AttachParameterByValue("Author", AuthorID);
                    DC.AttachParameterByValue("BlogContent", Content);

                    iRet = (int)Utils.ParseNumControlledReturn(DC.ExecuteScalar());

                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }

            }
            return iRet;
        }
        public string BlogProp(int ID, string Func, bool Value)
        {

            string sSQL = "UPDATE [dbo].[Blog] SET " + Func + "=@Func WHERE BlogID=@BlogID";
            string sRet = "Update Failed";
            using (Data DC = new Data("conn", Page, Process))
            {

                DC.AddCommand(CommandType.Text, sSQL);

                try
                {
                    DC.AttachParameterByValue("Func", Value);
                    DC.AttachParameterByValue("BlogID", ID);

                    int iRet = DC.ExecuteCommand();
                    if (iRet > 0)
                        sRet = "Update Successful!";
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }

            }
            return sRet;
        }
        public void GetPublicBlog()
        {
            List<Blog_Type> items = new List<Blog_Type>();
            string sSQL = "SELECT b.*, u.FirstName + ' ' + u.LastName AS AuthorName FROM [dbo].[Blog] b " +
                    "LEFT JOIN [dbo].[UserProfile] u ON u.userid = b.Author " +
                    "WHERE Parent = 0 AND ARCHIVE=0";

            using (Data DC = new Data("conn", Page, Process))
            {

                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();

                try
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int blogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]);
                        items.Add(new Blog_Type
                        {
                            BlogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]),
                            ParentID = (int)Utils.ParseNumControlledReturn(dr["Parent"]),
                            Archive = Utils.ParseBoolSafe(dr["Archive"]),
                            Author = (int)Utils.ParseNumControlledReturn(dr["Author"]),
                            AuthorName = dr["AuthorName"].StringSafe(),
                            Banned = Utils.ParseBoolSafe(dr["Banned"]),
                            BlogContent = Conversion.ConvertOut(dr["BlogContent"].StringSafe()),
                            BlogDate = Utils.ParseDateControlledReturn(dr["BlogDate"]),
                            BlogTrailID = blogID,
                            Sticky = Utils.ParseBoolSafe(dr["Sticky"]),
                            Title = dr["Title"].StringSafe(),
                            SubCategories = GetSubCategories(blogID, DC, true)
                        });
                    }
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }

                TheBlog = items;
            }
        }
        public void GetWholeBlog()
        {
            List<Blog_Type> items = new List<Blog_Type>();
            string sSQL = "SELECT b.*, u.FirstName + ' ' + u.LastName AS AuthorName FROM [dbo].[Blog] b " +
                    "LEFT JOIN [dbo].[UserProfile] u ON u.userid = b.Author " +
                    "WHERE Parent = 0";

            using (Data DC = new Data("conn", Page, Process))
            {

                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();

                try
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int blogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]);
                        items.Add(new Blog_Type
                        {
                            BlogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]),
                            ParentID = (int)Utils.ParseNumControlledReturn(dr["Parent"]),
                            Archive = Utils.ParseBoolSafe(dr["Archive"]),
                            Author = (int)Utils.ParseNumControlledReturn(dr["Author"]),
                            AuthorName = dr["AuthorName"].StringSafe(),
                            Banned = Utils.ParseBoolSafe(dr["Banned"]),
                            BlogContent = Conversion.ConvertOut(dr["BlogContent"].StringSafe()),
                            BlogDate = Utils.ParseDateControlledReturn(dr["BlogDate"]),
                            BlogTrailID = blogID,
                            Sticky = Utils.ParseBoolSafe(dr["Sticky"]),
                            Title = dr["Title"].StringSafe(),
                            SubCategories = GetSubCategories(blogID, DC, false)
                        });
                    }
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }

                TheBlog = items;
            }
        }
        private List<Blog_Type> GetSubCategories(int iMaster, Data DC, bool GetArchive)
        {
            if (iMaster == 0)
                return new List<Blog_Type>();

            string sSQL = "SELECT b.*, u.FirstName + ' ' + u.LastName AS AuthorName FROM [dbo].[Blog] b " +
                    "LEFT JOIN [dbo].[UserProfile] u ON u.userid = b.Author " +
                    "WHERE Parent = " + iMaster.ToString() + " {0} ORDER BY BlogTrailID";

            if (GetArchive)
                sSQL = string.Format(sSQL, "AND ARCHIVE = 0");
            else
                sSQL = string.Format(sSQL, "");


            List<Blog_Type> sub = new List<Blog_Type>();

            DC.AddCommand(CommandType.Text, sSQL);
            DataTable dt = DC.ExecuteCommandForDT();

            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int blogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]);
                    sub.Add(new Blog_Type
                    {
                        BlogID = (int)Utils.ParseNumControlledReturn(dr["BlogID"]),
                        ParentID = (int)Utils.ParseNumControlledReturn(dr["Parent"]),
                        Archive = Utils.ParseBoolSafe(dr["Archive"]),
                        Author = (int)Utils.ParseNumControlledReturn(dr["Author"]),
                        AuthorName = dr["AuthorName"].StringSafe(),
                        Banned = Utils.ParseBoolSafe(dr["Banned"]),
                        BlogContent = Conversion.ConvertOut(dr["BlogContent"].StringSafe()),
                        BlogDate = Utils.ParseDateControlledReturn(dr["BlogDate"]),
                        BlogTrailID = blogID,
                        Sticky = Utils.ParseBoolSafe(dr["Sticky"]),
                        Title = dr["Title"].StringSafe(),
                        SubCategories = GetSubCategories(blogID, DC, GetArchive)
                    });
                }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }

            return sub;
        }

        public int AddEntry(Blog_Type incoming)
        {
            string sSQL = "INSERT INTO [dbo].[Blog] ([Parent], [Title], [BlogDate], [Author], [BlogContent], [BlogTrailID], [Sticky], [Archive], [Banned]) " +
                 "VALUES (@Parent, @Title, getdate(), @Author, @BlogContent, (SELECT MAX(BlogTrailID) + 1 FROM [dbo].[Blog] WHERE Parent = @Parent), @Sticky, @Archive, @Banned); SELECT Scope_Identity();";

            int iRet = -1;
            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Parent", incoming.ParentID);
                    DC.AttachParameterByValue("Archive", incoming.Archive);
                    DC.AttachParameterByValue("Author", incoming.Author);
                    DC.AttachParameterByValue("Banned", incoming.Banned);
                    DC.AttachParameterByValue("BlogContent", Conversion.ConvertIn(incoming.BlogContent));
                    DC.AttachParameterByValue("Sticky", incoming.Sticky);
                    DC.AttachParameterByValue("Title", incoming.Title);
                    object obj = DC.ExecuteScalar();
                    iRet = (int)Utils.ParseNumControlledReturn(obj);
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }
            }
            return iRet;
        }

    }
}
