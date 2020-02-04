using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Web;
using System.IO;
using SwordsAndSorcery_2020.ModelTypes;
using DataTier.Repository;
using DataTier.Types;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class AccessModel
    {
        public UserType User { get; set; }
        public int UserID { get; set; }
        [Required(ErrorMessage = "A user name is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "A password is required.")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; }
        public string UserPassword2 { get; set; }
        public bool RememberMe { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email will be used to validate your log in.")]
        public string EMail { get; set; }
        public string ErrorMessage { get; set; }
        public bool GoHome { get; set; }

        public string UserToken { get; set; }

        public AccessModel()
        {
            GoHome = false;
        }

        public UserType FindUserByIP(string IPAddress)
        {
            User_Type user = new UserRepository(-1, "AccessModel", "FindUserByIP").GetUserByIPAddress(IPAddress);
            UserType _user = new UserType
            {
                Confirmed = user.Confirmed,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RememberMe = user.RememberMe,
                UserID = user.UserID,
                UserName = user.UserName
            };

            return _user;
        }

        public int GetUserIDForPassword(string Email, string UserName)
        {
            return new UserRepository(UserID, "AccessModel", "GetUserIDForPassword").GetUserIDForPassword(Email, UserName);
        }

        public string GetPasswordToken(int UserID)
        {
            UserRepository ur = new UserRepository(UserID, "AccessModel", "GetPasswordToken");
            string sToken = ur.GetPasswordToken();

            return sToken;
        }

        public string GetToken(int UserID)
        {
            //saves a new confirmation token
            string plug = EMail + DateTime.Today.ToShortDateString();
            string sToken = Encrypt(plug);
            sToken = Scrub(sToken);

            UserRepository ur = new UserRepository(UserID, "AccessModel", "GetToken");
            bool bRet = ur.SaveNewToken(UserID, sToken);

            if (bRet)
                return sToken;
            else
                return "Program Fail!";
        }

        public bool LogIn()
        {
            UserRepository ur = new UserRepository(-1, "AccessModel", "LogIN");
            User_Type user = ur.GetUser(UserName, Encrypt(UserPassword), Utils.GetIPAddress(), RememberMe);
            bool bRet = false;

            if (user != null)
                if (user.UserID > 0)
                {
                    User = new UserType
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        IPAddress = user.IPAddress,
                        LastName = user.LastName,
                        RememberMe = user.RememberMe,
                        RoleId = user.RoleId,
                        RoleName = user.RoleName,
                        UserID = user.UserID,
                        UserName = user.UserName,
                        Confirmed = user.Confirmed,
                        SessionStart = DateTime.Now
                    };
                    ur.LogIn();
                    bRet = true;
                }
            //record the login
            return bRet;
        }

        public bool LogOff(UserType User)
        {
            UserCache.RemoveFromCache(User.UserID, "");
            UserRepository ur = new UserRepository(User.UserID, "AccessModel", "Logoff");
            return ur.LogOut();
        }

        public int Register()
        {
            string plug = EMail + DateTime.Today.ToShortDateString();
            string sToken = Encrypt(plug);
            sToken = Scrub(sToken);
            User_Type user = new User_Type
            {
                Email = EMail,
                FirstName = FirstName,
                LastName = LastName,
                Pass = Encrypt(UserPassword),
                UserName = UserName,
                RememberMe = RememberMe,
                IPAddress = Utils.GetIPAddress(),
                Token = sToken
            };

            int iRet = new UserRepository(-1, "AccessModel", "Register").CreateUser(user);

            if (iRet > 1)
            {
                SendConfirmationEmail(sToken, iRet);
            }

            return iRet;
        }
        public bool UpdateUser()
        {
            User_Type user = new User_Type
            {
                Email = EMail,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                RememberMe = RememberMe,
                IPAddress = Utils.GetIPAddress(),
                UserID = UserID
            };
            return new UserRepository(UserID, "AccessModel", "UpdateUser").UpdateUser(user);
        }
        public void UpdatePassword()
        {
            UserRepository ur = new UserRepository(UserID, "AccessModel", "UpdatePassword");
            ur.SetNewPassword(Encrypt(UserPassword));
        }
        public bool SendConfirmationEmail(string sToken, int userID)
        {
            bool bRet = false;
            List<UserType> recipients = new List<UserType>();
            recipients.Add(new UserType
            {
                Email = EMail,
                UserName = UserName
            });

            List<UserType> cc = new List<UserType>();
            string subject = "Please confirm your membership to Swords and Sorcery";
            string body = "<a href=" + ConfigurationManager.AppSettings["SitePrefix"].ToString() + "Access/Confirmation?UserID=" + userID.ToString()
                + "&Token=" + sToken + ">Click Here To Register</a><p>Your Confirmation Code is: " + sToken + "</p>";

            Email mail = new Email();
            mail.UserID = userID;
            mail.Subject = subject;
            mail.Message = body;
            mail.SenderName = "thewizard";
            mail.FromAddress = "noreply@swordsandsorcery.com";
            mail.ToAddress = EMail;

            bRet = mail.SendEmail(true);

            if (!bRet)
            {
                //sometimes, the email is just greylisted and you need to send it again
                if(!mail.SendEmail(false))
                    ErrorMessage = "Email is down - a confirmation will be sent to you manually";
            }
            return bRet;
        }

        public bool SendPasswordEmail(int userID, string sToken)
        {
            bool bRet = false;
            List<UserType> recipients = new List<UserType>();
            recipients.Add(new UserType
            {
                Email = EMail,
                UserName = UserName
            });

            List<UserType> cc = new List<UserType>();
            string subject = "You forgot your password to Swords And Sorcery";
            string body = "<p>You are receiving this email because you couldn't remember your password to Swords And Sorcery.</p>" +
                "<a href=" + ConfigurationManager.AppSettings["SitePrefix"].ToString() + "Access/ProcessPassword?UserID=" + userID.ToString()
                + "&Token=" + sToken + ">Click Here To Create a New Password</a><p>Your Confirmation Code is: " + sToken + "</p>";

            Email mail = new Email();
            mail.UserID = userID;
            mail.Subject = subject;
            mail.Message = body;
            mail.SenderName = "thewizard";
            mail.FromAddress = "noreply@swordsandsorcery.com";
            mail.ToAddress = EMail;

            bRet = mail.SendEmail(true);

            if (!bRet)
                ErrorMessage = "Email is down - a confirmation will be sent to you manually";

            return bRet;
        }

        public string SetToken(int UserID)
        {
            //saves a new confirmation token
            string plug = EMail + DateTime.Today.ToShortDateString();
            string sToken = Encrypt(plug);
            sToken = Scrub(sToken);

            UserRepository ur = new UserRepository(UserID, "AccessModel", "SetToken");
            bool bRet = ur.SavePasswordToken(UserID, sToken);

            if (bRet)
                return sToken;
            else
                return "Program Fail!";
        }

        private string Scrub(string token)
        {
            string _out = token.Replace("+", "");
            _out = _out.Replace("%", "");
            _out = _out.Replace("&", "");
            _out = _out.Replace("?", "");
            _out = _out.Replace("+", "");
            _out = _out.Replace("/", "");
            _out = _out.Replace("=", "");

            return _out;
        }
        public bool Confirm()
        {
            if (UserToken.Length > 0)
                return new UserRepository(UserID, "AccessModel", "Confirm").VerifyUser(UserID, UserToken);
            else
                return false;
        }

        public string Encrypt(string clearText)
        {
            string EncryptionKey = "DISOEN58593KDHF";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = System.Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "DISOEN58593KDHF";
            byte[] cipherBytes = System.Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public UserType GetUserByUserID(int userID)
        {
            User_Type user = new UserRepository(userID, "AccessModel", "GetUserByID").GetUserByUserID(userID);
            UserType _user = new UserType();
            _user.CopyShallow(user);
            return _user;

        }
    }
}