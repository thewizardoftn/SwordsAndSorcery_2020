using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;
using DataTier.Types;
using DataTier.Repository;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class ProcessLogin
    {
        private int userID = 0;

        public int UserID { get { return userID; } set { userID = value; } }
        public string IPAddress { get; set; }
        public UserType User { get; set; }
        public bool DatabaseDown { get; set; }

        public ProcessLogin()
        {
            DatabaseDown = false;
        }

        public bool RememberMe(string ipAddress)
        {
            UserRepository u = new UserRepository(UserID, "ProcessLogin", "RememberMe");
            User_Type user = u.FindUser(ipAddress);

            DatabaseDown = u.DatabaseDown;

            if (user != null && user.UserID > 0)
            {
                UserType _user = new UserType
                {
                    Confirmed = user.Confirmed,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    IPAddress = user.IPAddress,
                    LastName = user.LastName,
                    Pass = true,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName,
                    SessionStart = DateTime.Now,
                    UserID = user.UserID,
                    UserName = user.UserName
                };
                User = _user;
            }

            return user.RememberMe;
        }

        public bool VerifyLogin(string UserName, string Password, bool RememberMe)
        {
            bool bRet = false;

            UserRepository u = new UserRepository(UserID, "ProcessLogin", "VerifyLogin");
            User_Type user = u.GetUser(UserName, Password, IPAddress, RememberMe);

            if (user != null && user.UserID > 0)
            {
                UserType _user = new UserType
                {
                    Confirmed = user.Confirmed,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    IPAddress = user.IPAddress,
                    LastName = user.LastName,
                    Pass = true,
                    RememberMe = user.RememberMe,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName,
                    SessionStart = DateTime.Now,
                    UserID = user.UserID,
                    UserName = user.UserName
                };
                User = _user;
                UserCache.RemoveFromCache(0, IPAddress);
                UserCache.AddToCache(_user);
                bRet = true;
            }

            return bRet;
        }
    }
}