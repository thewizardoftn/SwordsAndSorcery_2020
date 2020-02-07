using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class EmailRepository
    {
        public bool Result { get; set; }
        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }

        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public byte[] File { get; set; }

        public EmailRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
        }
        public void DeleteEmail(int emailID)
        {
            using (Data DC = new Data("conn", Page, Process))
            {

                string pre = "DELETE dbo.AttachedFiles WHERE MessageBankID=@MessageBankID";

                string sSQL = "DELETE dbo.MessageBank WHERE ID = @ID;";
                Result = false;
                try
                {
                    //if we have an attached email, we have to get rid of that first
                    DC.AddCommand(CommandType.Text, pre);
                    DC.AttachParameterByValue("MessageBankID", emailID);
                    DC.ExecuteCommand();
                    //now we can dump the email
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("ID", emailID);
                    int iRet = DC.ExecuteCommand();
                    if (iRet > 0)
                        Result = true;
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
        public void EmailWImage(string senderName, string senderEmail, string message, string fileName, byte[] file)
        {
            SenderName = senderName;
            SenderEmail = senderEmail;
            Message = message;
            FileName = fileName;
            File = file;
        }

        public bool CommitEmail()
        {
            bool bRet = false;
            string sSQL = "INSERT INTO dbo.AttachedFiles (MessageBankID, Descrip, FileName, Attachment) VALUES(@MessageBankID, @Descrip, @FileName, @Attachment);SELECT Scope_Identity();";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AttachParameterByValue("SenderName", SenderName);
                    DC.AttachParameterByValue("SenderEmail", SenderEmail);
                    DC.AttachParameterByValue("Subject", "Fovean Message");
                    DC.AttachParameterByValue("SentTime", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    DC.AttachParameterByValue("Message", Message);
                    int iRet = (int)Utils.ParseNumControlledReturn(DC.ExecuteScalar());

                    if (iRet > 0)
                    {
                        //now add the file
                        DC.AddCommand(System.Data.CommandType.Text, sSQL);
                        DC.AttachParameterByValue("MessageBankID", iRet);
                        DC.AttachParameterByValue("Descrip", Message);
                        DC.AttachParameterByValue("FileName", FileName);
                        DC.AttachFileIn("Attachment", File);
                        iRet = DC.ExecuteCommand();
                        if (iRet > 0)
                            bRet = true;
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

                return bRet;
            }
        }
        public void PreserveEmail(string SenderEmail, string SenderName, string Subject, string Message, int UserID, string IP)
        {
            string sSQL = "INSERT INTO dbo.MessageBank (SenderEmail, SenderName, Subject, SentTime, Message, IP) VALUES(@SenderEmail, @SenderName, @Subject, getdate(), @Message, @IP) ";
            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("SenderEmail", SenderEmail);
                    DC.AttachParameterByValue("SenderName", SenderName);
                    DC.AttachParameterByValue("Subject", Subject.StringSafe());
                    DC.AttachParameterByValue("Message", Message);
                    DC.AttachParameterByValue("IP", IP);
                    int iRet = DC.ExecuteCommand();
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

        public string SaveEmail(string email, string name)
        {
            string sSQL = "INSERT INTO dbo.EMails (Email, Name) VALUES(@Email, @Name) ";
            string sRet = "";
            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Email", email);
                    DC.AttachParameterByValue("Name", name);
                    sRet = DC.ExecuteCommand().StringSafe();
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
    }
}
