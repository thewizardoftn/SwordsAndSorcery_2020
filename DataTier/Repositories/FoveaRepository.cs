using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class FoveaRepository
    {
        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public List<FoveaTerms_Type> Terms { get; set; }

        public FoveaRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
        }
        public void FoveaTerms()
        {
            List<FoveaTerms_Type> terms = new List<FoveaTerms_Type>();
            using (Data DC = new Data("conn", Page, Process))
            {

                DataTable dt = new DataTable();
                string sSQL = "SELECT * FROM dbo.Fovea WHERE Approved = 1 ORDER BY Subject;";
                DC.AddCommand(CommandType.Text, sSQL);
                try
                {
                    dt = DC.ExecuteCommandForDT();
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        terms.Add(new FoveaTerms_Type
                        {
                            FoveaID = (int)Utils.ParseNumControlledReturn(dr["FoveaID"]),
                            Approved = Utils.ParseBoolSafe(dr["Approved"]),
                            Description = dr["Description"].ToString(),
                            Subject = dr["Subject"].ToString()
                        });
                    }
                }
            }
            Terms = terms;
        }
        public bool AddTerm(string Subject, string Content)
        {
            bool bRet = false;

            string sSQL = "INSERT INTO dbo.Fovea (Subject, Description, Approved) VALUES (@Subject, @Description, 0);";

            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Subject", Subject.StringSafe());
                    DC.AttachParameterByValue("Description", Content);
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
                return bRet;
            }
        }
        public bool AddApprovedTerm(string Subject, string Content)
        {
            bool bRet = false;

            string sSQL = "INSERT INTO dbo.Fovea (Subject, Description, Approved) VALUES (@Subject, @Description, 1);";

            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Subject", Subject.StringSafe());
                    DC.AttachParameterByValue("Description", Content);
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
        public bool RemoveTerm(int FoveaID)
        {
            bool bRet = false;

            string sSQL = "DELETE dbo.Fovea WHERE FoveaID=@FoveaID;";

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("FoveaID", FoveaID);
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
        public List<FoveaTerms_Type> UnapprovedTerms()
        {
            List<FoveaTerms_Type> terms = new List<FoveaTerms_Type>();

            using (Data DC = new Data("conn", Page, Process))
            {

                DataTable dt = new DataTable();
                string sSQL = "SELECT * FROM dbo.Fovea WHERE Approved = 0 OR Approved is NULL ORDER BY Subject;";
                DC.AddCommand(CommandType.Text, sSQL);
                try
                {
                    dt = DC.ExecuteCommandForDT();
                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, Process, sSQL);

                }
                finally
                {
                    DC.Dispose();
                }
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        terms.Add(new FoveaTerms_Type
                        {
                            FoveaID = (int)Utils.ParseNumControlledReturn(dr["FoveaID"]),
                            Approved = Utils.ParseBoolSafe(dr["Approved"]),
                            Description = dr["Description"].ToString(),
                            Subject = dr["Subject"].ToString()
                        });
                    }
                }
            }
            return terms;
        }
        public bool UpdateTerm(int FoveaID, string Subject, string Content)
        {
            bool bRet = false;

            string sSQL = "Update dbo.Fovea SET Approved = 1, Subject = @Subject, Description=@Description WHERE FoveaID=@FoveaID;";

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("FoveaID", FoveaID);
                    DC.AttachParameterByValue("Subject", Subject);
                    DC.AttachParameterByValue("Description", Content);

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
    }
}
