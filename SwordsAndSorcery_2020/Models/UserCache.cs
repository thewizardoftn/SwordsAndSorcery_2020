using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using System.Data;
using DataTier.Repository;
using DataTier.Types;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public static class UserCache
    {
        private static DateTime cacheStart = DateTime.Now;
        private static MemoryCache _cache = MemoryCache.Default;

        public static DateTime CacheStart { get { return cacheStart.CorrectTimeZone(); } }

        public static List<UserType> Users
        {
            get
            {
                if (!_cache.Contains("SSUsers"))
                {
                    List<UserType> endUsers = new List<UserType>();
                    SaveEndUsers(endUsers);
                }
                return _cache.Get("SSUsers") as List<UserType>;

            }
        }
        public static int Count()
        {
            List<UserType> users = new List<UserType>();
            try
            {
                users = _cache.Get("SSUsers") as List<UserType>;
                if (users == null)
                    users = new List<UserType>();
            }
            catch { }

            return users.Count;
        }

        public static Dictionary<string, string> Output()
        {
            Dictionary<string, string> users = new Dictionary<string, string>();
            try
            {
                List<UserType> _out = (List<UserType>)_cache.Get("SSUsers");
                foreach(var item in _out)
                {
                    users.Add(item.UserName, item.IPAddress);
                }
            }
            catch { }
            return users;
        }
        public static void AddToCache(UserType u)
        {
            List<UserType> endUsers = new List<UserType>();

            if (!_cache.Contains("SSUsers"))
                SaveEndUsers(endUsers);

            endUsers = (List<UserType>)_cache.Get("SSUsers");

            UserType ut = endUsers.Find(x => x.UserID == u.UserID);
            if (ut != null)
            {
                endUsers.Remove(ut);
            }



            endUsers.Add(u);
            SaveEndUsers(endUsers);
        }

        public static UserType BuildUser(int UserID, string IPAddress, string Page)
        {
            UserType user = new UserType
            {
                UserName = "Anonymous",
                UserID = -1,
                Confirmed = true
            };

            return user;
        }

        public static void Clear(int UserID)
        {
            List<UserType> endUsers = new List<UserType>();

            if (!_cache.Contains("SSUsers"))
                return;

            endUsers = (List<UserType>)_cache.Get("SSUsers");
            UserType ut = endUsers.Find(u => u.UserID == UserID);
            if (ut == null)
                return;

            endUsers.Remove(ut);
            endUsers.Add(ut);
            SaveEndUsers(endUsers);

        }

        public static void Dispose()
        {
            _cache.Remove("SSUsers");
        }

        public static UserType GetFromCache(int UserID, string IPAddress)
        {
            UserType u = new UserType();
            try
            {

                // we're getting a user from cache.
                if (UserID < 1)
                    u = findInCache(IPAddress);
                else
                    u = findInCacheByUserID(UserID);
                //if this comes back null, you're not logged in
                if (u == null)
                    u = new UserType();

                switch (u.UserID >0)
                {
                    case true:
                        //this person was in the cache
                        AddToCache(u);
                        break;
                    case false:
                        //this person could be in the cache
                        if (u == null)
                        {
                            List<UserType> endUsers = new List<UserType>();
                            try
                            {
                                endUsers = (List<UserType>)_cache.Get("SSUsers");
                                if (endUsers == null)
                                {
                                    //there is no user in cache, see if the user is in the database
                                    u = AnonymousUser(IPAddress);
                                }
                                else
                                {
                                    User_Type _u = new UserRepository(UserID, "UserCache", "GetFromCache").GetUserByIPAddress(HttpContext.Current.Request.UserHostAddress);
                                    u.CopyShallow(_u);
                                }
                            }
                            catch { }
                            AddToCache(u);
                        }
                        else
                        {
                            //last chance - see if this person is in the database
                            u = FindByIPAddress(IPAddress);
                            AddToCache(u);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            return u;
        }

        public static UserType findInCache(string IPAddress)
        {
            //returns a user or a null
            UserType u = null;
            try
            {
                List<UserType> endUsers = (List<UserType>)_cache.Get("SSUsers");
                if (endUsers != null)
                    u = endUsers.Find(x => x.IPAddress == IPAddress);

                if (u != null)
                    if (u.UserID == 0)
                        u = null;
            }
            catch { }

            return u;
        }
        public static UserType findInCacheByUserID(int UserID)
        {
            UserType u = null;
            try
            {
                List<UserType> endUsers = (List<UserType>)_cache.Get("SSUsers");
                if (endUsers != null)
                    u = endUsers.Find(x => x.UserID == UserID);

                if (u != null)
                    if (u.UserID == 0)
                        u = null;
            }
            catch { }

            return u;
        }
        private static UserType FindByIPAddress(string IPAddress)
        {
            //search the database for an IP address which is Remember me
            UserType u = new UserType();
            ProcessLogin log = new ProcessLogin();
            if (log.RememberMe(IPAddress))
            {
                u = log.User;
            }
            else if (log.DatabaseDown)
            {
                return null;
            }
            else
                u = AnonymousUser(IPAddress);

            return u;
        }
        private static UserType AnonymousUser(string IPAddress)
        {
            return new UserType
            {
                Email = "",
                FirstName = "Anonymous",
                LastName = "User",
                UserID = -1,
                IPAddress = IPAddress,
                RoleId = 0,
                SessionStart = DateTime.Now,
                UserName = "Anonymous"
            };
        }
        public static bool isInCache(int UserID)
        {
            bool bRet = false;
            UserType u = null;
            try
            {
                List<UserType> endUsers = (List<UserType>)_cache.Get("SSUsers");
                if (endUsers != null)
                    u = endUsers.Find(x => x.UserID == UserID);


                if (u != null)
                    if (u.UserID >0)
                        bRet = true;
            }
            catch { }

            return bRet;
        }

        public static void RemoveFromCache(int UserID, string ipAddress)
        {
            List<UserType> endUsers = new List<UserType>();

            try
            {
                if (!_cache.Contains("SSUsers"))
                    return;
                else
                    endUsers = (List<UserType>)_cache.Get("SSUsers");
                if (UserID > 0)
                {
                    var remove = endUsers.Single(m => m.UserID == UserID);
                    endUsers.Remove(remove);
                }
                else
                {
                    var remove = endUsers.Single(m => m.IPAddress == ipAddress);
                    endUsers.Remove(remove);
                }
                SaveEndUsers(endUsers);
            }
            catch { }
        }

        public static void SaveEndUsers(List<UserType> endUsers)
        {

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now.AddDays(1);
            _cache = MemoryCache.Default;
            _cache.Add("SSUsers", endUsers, policy);
        }

        public static void Update(int UserID, string Page, bool Confirmed)
        {
            List<UserType> endUsers = new List<UserType>();

            try
            {
                if (!_cache.Contains("SSUsers"))
                    return;
                else
                    endUsers = (List<UserType>)_cache.Get("SSUsers");


                UserType u = endUsers.Find(m => m.UserID == UserID);
                endUsers.Remove(u);

                u.IPAddress = Page;
                u.Confirmed = Confirmed;

                endUsers.Add(u);
                SaveEndUsers(endUsers);
            }
            catch { }
        }

        public static void SetFooter(int ID, bool minimize)
        {
            List<UserType> endUsers = new List<UserType>();

            try
            {
                if (!_cache.Contains("SSUsers"))
                    return;
                else
                    endUsers = (List<UserType>)_cache.Get("SSUsers");


                UserType u = endUsers.Find(m => m.UserID == ID);
                endUsers.Remove(u);

                u.DownArrow = minimize;

                endUsers.Add(u);
                SaveEndUsers(endUsers);
            }
            catch { }
        }
    }
}