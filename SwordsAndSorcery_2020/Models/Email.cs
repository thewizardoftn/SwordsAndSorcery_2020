using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Mail;
using DataTier.Repository;
using DataTier.Types;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class Email
    {
        public int UserID { get; set; }
        public string SenderName { get; set; }
        public string ToAddress
        {
            get { return toAddress; }
            set { toAddress = value; }
        }
        public string FromAddress { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public byte[] AttachedFile { get; set; }
        public string IP { get; set; }

        private string toAddress = ConfigurationManager.AppSettings.Get("EmailAddress");
        private string UserName = ConfigurationManager.AppSettings.Get("SMTPUser");
        private string SMTPServer = ConfigurationManager.AppSettings.Get("EMailServer");
        private string Password = ConfigurationManager.AppSettings.Get("SMTPPass");
        private string ReplyTo = ConfigurationManager.AppSettings.Get("AccountEmail");
        private int SMTPPort = (int)Utils.ParseNumControlledReturn(ConfigurationManager.AppSettings.Get("SMTPPort"));
        private string enableSSL = ConfigurationManager.AppSettings.Get("EnableSSL");


        public Email()
        {
            IP = HttpContext.Current.Request.UserHostAddress;
            UserType user = UserCache.GetFromCache(0, IP);
            if (user == null)
                return;                     //this is an indicator that the database is down

            UserID = user.UserID;
        }
        public bool DeleteEmail(int emailID)
        {
            bool bRet = false;
            EmailRepository d = new EmailRepository(UserID, "Email", "DeleteEmail");
            d.DeleteEmail(emailID);
            bRet = d.Result;

            return bRet;
        }
        public bool SaveFeedback()
        {
            EmailRepository e = new EmailRepository(UserID, "Email", "SaveFeedBack");
            e.EmailWImage(SenderName, FromAddress, Message, FileName, AttachedFile);
            return e.CommitEmail();
        }
        private void SaveEmail()
        {
            EmailRepository e = new EmailRepository(UserID, "Email", "SaveEmail");
            if (toAddress == null || Subject == null)
            {
                SaveError error = new SaveError();
                ErrorType err = new ErrorType
                {
                    Err_Message = "Subject Missing Error",
                    LiteralDesc = e.Message,
                    SQL = "",
                    Page = "Email.cs",
                    Process = "SaveEmail",
                    Err_Subject = "Tracking this error - null subject or toAddress",
                    UserID = UserID
                };
                error.ReportError(err);
            }
            e.PreserveEmail(toAddress, SenderName, Subject, Message, UserID, IP);
        }
        public bool SendAlert(bool save, string Body, string Email, string subject)
        {
            bool bRet = false;
            bool bEnableSSL = Utils.ParseBoolSafe(enableSSL);
            if (save)
                SaveEmail();

            try
            {
                MailMessage mail = new MailMessage(ToAddress, Email);
                mail.Subject = subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;


                ServicePointManager.Expect100Continue = false;
                SmtpClient sc = new SmtpClient();
                sc.Host = SMTPServer;
                sc.EnableSsl = bEnableSSL;
                NetworkCredential cred = new NetworkCredential(UserName, Password);
                sc.UseDefaultCredentials = true;
                sc.Credentials = cred;
                sc.Port = SMTPPort;
                sc.Send(mail);
                bRet = true;
            }
            catch (Exception ex)
            {
                SaveError error = new SaveError();
                ErrorType err = new ErrorType
                {
                    Err_Message = ex.Message,
                    Err_Subject = "Email Failure",
                    Page = "Email.cs",
                    Process = "SendAlert",
                    UserID = UserID,
                    LiteralDesc = ex.StackTrace.ToString()
                };
                error.ReportError(err);
            }

            return bRet;
        }
        public bool SendEmail(bool save)
        {
            bool bRet = false;
            bool bEnableSSL = Utils.ParseBoolSafe(enableSSL);

            if (save)
                SaveEmail();

            try
            {
                MailMessage mail = new MailMessage(FromAddress, ToAddress);
                mail.Subject = Subject;
                mail.Body = Message;
                mail.IsBodyHtml = true;

                if (AttachedFile != null)
                {
                    Stream stream = new MemoryStream(AttachedFile);
                    mail.Attachments.Add(new Attachment(stream, FileName));
                }
                ServicePointManager.Expect100Continue = false;
                SmtpClient sc = new SmtpClient();
                sc.Host = SMTPServer;
                sc.EnableSsl = bEnableSSL;
                NetworkCredential cred = new NetworkCredential(UserName, Password);
                sc.UseDefaultCredentials = true;
                sc.Credentials = cred;
                sc.Port = SMTPPort;
                sc.Send(mail);
                bRet = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Greylisted"))
                {
                    System.Threading.Thread.Sleep(60000);
                    SendEmail(false);
                }
                SaveError error = new SaveError();
                ErrorType err = new ErrorType
                {
                    Err_Message = ex.Message,
                    Err_Subject = "Email Failure",
                    Page = "Email.cs",
                    Process = "SendEmail",
                    UserID = UserID,
                    LiteralDesc = ex.StackTrace.ToString()
                };

                error.ReportError(err);
            }

            return bRet;
        }
        private string MakeMessage()
        {
            string ret = "<HTML><BODY><p>" +
                "From: " + FromAddress + ", " + SenderName + "</p><p>" +
                "Message: " + Message + "</p></BODY></HTML>";
            return ret;
        }
        public byte[] GetFile(string location)
        {
            string fileName = Path.GetFileName(location);
            MemoryStream target = new MemoryStream();
            StreamReader sr = new StreamReader(location);
            sr.BaseStream.CopyTo(target);
            byte[] data = target.ToArray();
            return data;
        }
    }
}