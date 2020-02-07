using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class AdminRepository
    {
        private List<PageHitType> hits;
        private List<UserSpec> users;

        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public Dictionary<string, string> Requests { get; set; }
        public List<Error_Type> ErrorList { get; set; }
        public List<EmailDataType> Emails { get; set; }
        public List<IPSpec> IPS { get; set; }
        public List<PageHitType> Hits
        {
            get { return hits; }
            set { hits = value; }
        }
        public List<FoveaTerms_Type> Terms { get; set; }
        public List<UserSpec> Users
        {
            get { return users; }
            set { users = value; }
        }
        public AdminRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
        }
        public void GetCollections()
        {
            using (Data DC = new Data("conn", Page, Process))
            {
                GetBookReqs(DC);
                GetEmails(DC);
                GetPageHits(DC);
                GetErrors(DC);
                GetFoveaTerms(DC);
                TopFiveIPS(DC);
                LastFiveUsers(DC);
                DC.Dispose();
            }
        }
        public void GetBookReqs(Data DC)
        {
            string sSQL = "SELECT * FROM [dbo].[EMails] Order by Name";
            Requests = new Dictionary<string, string>();
            try
            {
                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        Requests.Add(value: dr["Email"].ToString(),
                            key: dr["Name"].ToString());
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }
        }

        public void GetEmails(Data DC)
        {
            DataTable dt = new DataTable();
            List<EmailDataType> emails = new List<EmailDataType>();

            string sSQL = "SELECT * FROM MessageBank Order by SentTime DESC";
            try
            {
                DC.AddCommand(CommandType.Text, sSQL);
                dt = DC.ExecuteCommandForDT();
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        emails.Add(new EmailDataType
                        {
                            ID = (int)Utils.ParseNumControlledReturn(dr["ID"]),
                            Message = dr["Message"].ToString(),
                            SenderEmail = dr["SenderEmail"].ToString(),
                            SenderName = dr["SenderName"].ToString(),
                            SentTime = Utils.ParseDateControlledReturn(dr["SentTime"].ToString()),
                            Subject = dr["Subject"].ToString(),
                            
                        });
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }

            Emails = emails;
        }
        public bool AddToDisAllowed(string IP)
        {
            string sSQL = "insert into [dbo].[DisAllowed]([IP]) VALUES(@IP)";
            bool bRet = false;

            using (Data DC = new Data("conn", Page, Process))
            {

                DataTable dt = new DataTable();
                List<EmailDataType> emails = new List<EmailDataType>();

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("IP", IP);
                    int iRet = DC.ExecuteCommand();
                    if (iRet > 0)
                        bRet = true;
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
            return bRet;
        }
        public void GetErrors(Data DC)
        {
            
            DataTable dt = new DataTable();
            List<Error_Type> errors = new List<Error_Type>();

            string sSQL = "SELECT * FROM ErrorTable Order by Time_Of_Error DESC";
            try
            {
                DC.AddCommand(CommandType.Text, sSQL);
                dt = DC.ExecuteCommandForDT();
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        errors.Add(new Error_Type
                        {
                            ErrorID = (int)Utils.ParseNumControlledReturn(dr["ErrorID"]),
                            Err_Message = dr["Err_Message"].ToString(),
                            Err_Subject = dr["Err_Subject"].ToString(),
                            LiteralDesc = dr["LiteralDesc"].ToString(),
                            Page = dr["Page"].ToString(),
                            Process = dr["Process"].ToString(),
                            SQL = dr["SQL"].ToString(),
                            Time_Of_Error = Utils.ParseDateControlledReturn(dr["Time_Of_Error"]),
                            UserName = dr["UserName"].ToString(),
                            Corrected = Utils.ParseBoolSafe(dr["Corrected"])
                        });
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }

            ErrorList = errors;
        }
        public void GetFoveaTerms(Data DC)
        {
            List<FoveaTerms_Type> terms = new Fovea_Terms(UserID, "AdminRepository", "GetFoveaTerms").UnapprovedTerms();
            Terms = terms;
        }
        public void GetPageHits(Data DC)
        {
            string sSQL = "SELECT HitDate, PageName, sum(Hits)AS Hits FROM PageHits WHERE HitDate > GETDATE()-5 AND IP != '50.142.42.118' GROUP BY HitDate, PageName ORDER BY HitDate DESC, Hits DESC";
            try
            {
                hits = new List<PageHitType>();
                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();

                if (dt != null)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            hits.Add(new PageHitType
                            {
                                HitDate = Utils.ParseDateControlledReturn(dr["HitDate"]),
                                PageName = dr["PageName"].ToString(),
                                Hits = (int) Utils.ParseNumControlledReturn(dr["Hits"])
                            });
                        }
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }

        }
        public void LastFiveUsers(Data DC)
        {
            string sSQL = "SELECT TOP 5 u.UserID, UserName, CreateDate, IsConfirmed from dbo.UserProfile u " + 
	            "join dbo.webpages_Membership m on m.UserId = u.UserID ORDER BY CreateDate DESC";
            try
            {
                users = new List<UserSpec>();
                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();

                if (dt != null)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            users.Add(new UserSpec
                            {
                                UserName = dr["UserName"].ToString(),
                                UserID = (int) Utils.ParseNumControlledReturn(dr["UserID"]),
                                CreateDate = Utils.ParseDateControlledReturn(dr["CreateDate"]),
                                IsConfirmed = Utils.ParseBoolSafe(dr["IsConfirmed"])
                            });
                        }
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }
        }
        public void TopFiveIPS(Data DC)
        {
            string sSQL = "SELECT TOP 20 IP, COUNT(Hits) AS Hits FROM  PageHits WHERE IP NOT IN (SELECT IP FROM [dbo].[DisAllowed]) GROUP BY IP ORDER BY HITS DESC";
            try
            {
                IPS = new List<IPSpec>();
                DC.AddCommand(CommandType.Text, sSQL);
                DataTable dt = DC.ExecuteCommandForDT();

                if (dt != null)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            IPS.Add(new IPSpec
                            {
                                IP = dr["IP"].ToString(),
                                Hits = (int)Utils.ParseNumControlledReturn(dr["Hits"])
                            });
                        }
                    }
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }
        }
    }
}
