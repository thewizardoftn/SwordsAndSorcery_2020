using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class FeedbackRepository
    {
        public object O { get; set; }
        public int UserID{get; set; }
        public string Page { get; set; }
        public string Process { get; set; }


        public FeedbackRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
        }


        public void Feedback(string SenderName, string SenderEmail, string Message, string IP)
        {
            O = -1;
            string sSQL = "INSERT INTO MessageBank (SenderName, SenderEmail, Subject, IP, SentTime, Message) VALUES(@SenderName, @SenderEmail, @Subject, @IP, getdate(), @Message); SELECT Scope_Identity();";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(System.Data.CommandType.Text, sSQL);
                    DC.AttachParameterByValue("SenderName", SenderName);
                    DC.AttachParameterByValue("SenderEmail", SenderEmail);
                    DC.AttachParameterByValue("Subject", "Fovean Message");
                    DC.AttachParameterByValue("IP", IP);
                    DC.AttachParameterByValue("Message", Message);

                    object oRet = DC.ExecuteScalar();
                    if (oRet != null)
                        O = oRet;
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
        }
    }
}
