using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SwordsAndSorcery_2020.Models;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Controllers
{
    public class AccessController : Controller
    {
        public ActionResult ForgotPassword(int UserID)
        {
            UserType user = new AccessModel().GetUserByUserID(UserID);
            return View(user);
        }
        [HttpPost]
        public ActionResult ProcessPassword(UserType model)
        {
            AccessModel mod = new AccessModel();
            int UserID = mod.GetUserIDForPassword(model.Email, model.UserName);
            if (UserID > 0)
            {
                string sToken = mod.SetToken(UserID);
                mod.UserName = model.UserName;
                mod.UserID = UserID;
                mod.EMail = model.Email;
                mod.SendPasswordEmail(UserID, sToken);

                return RedirectToAction("index", "home");
            }
            else
            {
                mod.ErrorMessage = "Can't find that email or user name";
                return View("Login", model);
            }
        }
        public ActionResult ProcessPassword(int UserID, string Token)
        {
            AccessModel model = new AccessModel();
            string _token = model.GetPasswordToken(UserID);

            if (_token == Token || _token.ToUpper() == Token)
            {
                UserType user = model.GetUserByUserID(UserID);
                UserCache.AddToCache(user);

                return RedirectToAction("Manage", new { UserID = UserID });
            }
            else
            {
                model.ErrorMessage = "The token did not match";
                return View("Login", model);
            }
        }

        public ActionResult Login()
        {
            AccessModel model = new AccessModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Login(AccessModel model)
        {
            if (!ModelState.IsValid)
            {
                string sError = "";
                foreach (var item in ModelState)
                {
                    if (sError.Length > 0)
                        sError += ", ";
                    if (item.Value.Errors.Count > 0 && !item.Value.Errors[0].ErrorMessage.Contains("mail"))
                        sError += item.Value;
                }
                if (sError.Length > 0)
                {
                    model.ErrorMessage = sError;
                    return View(model);
                }

            }
            bool bRet = model.LogIn();
            if (bRet)
            {
                UserCache.RemoveFromCache(0, model.User.IPAddress);
                UserCache.AddToCache(model.User);
                model.GoHome = true;
            }
            //if you made it this far, the login didn't work
            return View(model);
        }
        public ActionResult LogOff(int userID)
        {
            string ip = "";

            if (ViewBag.IPAddress != null)
                ip = ViewBag.IPAddress.StringSafe(); 
            
            UserType user = UserCache.GetFromCache(userID, ip);
            AccessModel model = new AccessModel();
            UserType _user = new UserType();

            if (user != null)
            {
                _user = new UserType
                {
                    UserName = "Anonymous",
                    UserID = -1,
                    SessionStart = DateTime.Now
                };
            }
            bool bRet = model.LogOff(user);
            if (bRet)
            {
                UserCache.RemoveFromCache(user.UserID, "");
                UserCache.AddToCache(_user);
            }
            return RedirectToAction("index", "home");
        }
        public ActionResult Manage(int UserID)
        {
            AccessModel model = new AccessModel();
            UserType user = UserCache.GetFromCache(UserID, "");
            model.User = user;
            model.UserID = user.UserID;
            model.UserName = user.UserName;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.EMail = user.Email;

            return View(model);
        }
        [HttpPost]
        public ActionResult Manage(AccessModel model)
        {
            if (model.UserPassword != null)
                if (model.UserPassword == model.UserPassword2)
                {
                    model.UpdatePassword();
                }
                else
                {
                    model.ErrorMessage = "The passwords don't match!";
                    return View(model);
                }
            if (model.UpdateUser())
                return RedirectToAction("Index", "Home");
            else
            {
                model.ErrorMessage = "Something went wrong!";
                return View(model);
            }
        }
        public ActionResult NewPassword(AccessModel model)
        {
            if (model.UserPassword == model.UserPassword2)
            {
                model.UpdatePassword();

                return RedirectToAction("index", "home");
            }
            else
            {
                model.ErrorMessage = "The passwords don't match!";
                return View(model);
            }
        }
        public ActionResult Register()
        {
            AccessModel model = new AccessModel();

            return View(model);
        }
        [HttpPost]
        public ActionResult Register(AccessModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            else
            {
                int iRet = model.Register();
                if (iRet > 0)
                {
                    return RedirectToAction("index", "home");

                }
                else if (iRet == -5)
                {
                    model.ErrorMessage = "This user name or email is taken";
                }
                else
                    model.ErrorMessage = "You failed to log in";
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Confirmation(AccessModel model)
        {
            if (model.UserID < 1)
            {
                UserType user = UserCache.GetFromCache(0, ViewBag.IPAddress.StringSafe());
                model.UserID = user.UserID;
            }
            bool bRet = model.Confirm();

            if (bRet)
            {
                UserCache.Update(model.UserID, "index", true);
                return RedirectToAction("index", "home");
            }
            else if (model.UserToken.Length > 0)
            {
                model.ErrorMessage = "Confirmation Code Failed!";
                SaveError error = new SaveError();
                ErrorType err = new ErrorType
                {
                    Err_Message = "Confirmation Error",
                    LiteralDesc = model.UserToken,
                    SQL = model.GetToken(model.UserID),
                    Page = "Confirmation",
                    Process = "Confirmation",
                    Err_Subject = "User could not confirm",
                    UserID = model.UserID
                };
                error.ReportError(err);
            }
            else
                model.ErrorMessage = "Click Resend to receive a new confirmation message";

            return View(model);
        }
        public ActionResult Confirmation(int UserID, string token)
        {
            AccessModel model = new AccessModel();

            token = token.StringSafe();                 //eliminate nulls

            model.UserID = UserID;
            model.UserToken = token;
            bool bRet = false;

            model.ErrorMessage = "If you no longer have a code, click the 'Resend' button";

            if (token.Length > 0)
                bRet = model.Confirm();


            if (bRet)
            {
                UserType user = new AccessModel().GetUserByUserID(UserID);
                if (user != null)
                {
                    UserCache.AddToCache(user);
                    UserCache.findInCache(ViewBag.IPAddress.StringSafe());
                }
                return RedirectToAction("index", "home");
            }
            else if (model.UserToken.Length > 0)
                model.ErrorMessage = "Confirmation Code Failed!";
            else
                model.ErrorMessage = "Click Resend to receive a new confirmation message";

            return View(model);
        }
        public ActionResult Resend()
        {
            AccessModel model = new AccessModel();
            UserType user = UserCache.GetFromCache(0, ViewBag.IPAddress.StringSafe());
            int UserID = user.UserID;

            string sToken = model.GetToken(UserID);
            model.EMail = user.Email;
            model.UserName = user.UserName;

            model.SendConfirmationEmail(sToken, UserID);

            return RedirectToAction("index", "home");
        }
    }
}