using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class PageHits
    {
        public string IP { get; set; }
        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }

        public PageHits(string page, string ip, int userID, string process)
        {
            Page = page;
            Process = process;
            IP = ip;
            UserID = userID;
        }

        public bool DoUpdate()
        {
            bool bRet = false;

            string sSQL = "INSERT INTO dbo.PageHits (PageName, IP, HitDate, Hits) VALUES(@PageName, @IP, getdate(), 1);";
            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(System.Data.CommandType.Text, sSQL);
                    DC.AttachParameterByValue("PageName", Page);
                    DC.AttachParameterByValue("IP", IP);
                    int i = DC.ExecuteCommand();
                    if (i > 0)
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


        public bool NeedUpdate()
        {
            bool bRet = false;

            string sSQL = "SELECT Count(HitID) FROM dbo.PageHits WHERE PageName=@PageName AND IP=@IP AND HitDate=CONVERT(date, getdate());";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(System.Data.CommandType.Text, sSQL);
                    DC.AttachParameterByValue("PageName", Page);
                    DC.AttachParameterByValue("IP", IP);
                    object oRet = DC.ExecuteScalar();
                    int iRet = (int)Utils.ParseNumControlledReturn(oRet);
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
