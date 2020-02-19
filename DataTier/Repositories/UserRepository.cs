using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataTier.Types;
using Utilities;

namespace DataTier.Repository
{
    public class UserRepository
    {
        public int UserID { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public bool DatabaseDown { get; set; }

        public UserRepository(int userID, string page, string process)
        {
            Page = page;
            Process = process;
            UserID = userID;
            DatabaseDown = false;
        }

        public int CreateUser(User_Type type)
        {
            string sSQL = "INSERT INTO [dbo].[UserProfile] ([UserName], [IPAddress], [FirstName], [LastName], [Email], RememberMe, [LastLogin]) " +
                "VALUES (@UserName, @IPAddress, @FirstName, @LastName, @Email, @RememberMe, getdate());SELECT Scope_Identity();";

            int iRet = -5;

            if (!VerifyUser(type.UserName, type.Email))
            {
                using (Data DC = new Data("conn", Page, Process))
                {

                    DC.Persistant = true;

                    try
                    {
                        DC.AddCommand(CommandType.Text, sSQL);
                        DC.AttachParameterByValue("UserName", type.UserName);
                        DC.AttachParameterByValue("IPAddress", type.IPAddress);
                        DC.AttachParameterByValue("FirstName", type.FirstName);
                        DC.AttachParameterByValue("LastName", type.LastName);
                        DC.AttachParameterByValue("Email", type.Email);
                        DC.AttachParameterByValue("RememberMe", type.RememberMe);

                        object obj = DC.ExecuteScalar();
                        iRet = (int)Utils.ParseNumControlledReturn(obj);

                        if (iRet > 0)
                        {
                            sSQL = "INSERT INTO [dbo].[webpages_Membership] ([UserID], [CreateDate], [ConfirmationToken], [Password], [PasswordChangedDate], [PasswordFailuresSinceLastSuccess], [PasswordSalt]) " +
                                "VALUES (@UserID, getdate(), @Token, @Password, getdate(), 0, '' )";
                            DC.AddCommand(CommandType.Text, sSQL);
                            DC.AttachParameterByValue("UserID", iRet);
                            DC.AttachParameterByValue("Token", type.Token);
                            DC.AttachParameterByValue("Password", type.Pass);

                            int iNew = DC.ExecuteCommand();
                            if (iNew > 0)
                            {
                                sSQL = "INSERT INTO [dbo].[webpages_UsersInRoles] (UserID, RoleID) VALUES(@UserID, 5)";
                                DC.AddCommand(CommandType.Text, sSQL);
                                DC.AttachParameterByValue("UserID", iRet);

                                int iNewer = DC.ExecuteCommand();

                                if (iNewer > 0)
                                    DC.CommitPersistant();
                                else
                                    iRet = -2;
                            }
                            else
                                iRet = -3;
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
                }
            }
            return iRet;
        }

        public bool EmailExists(string EMail)
        {
            string sSQL = "SELECT Count(UserID) FROM [dbo].[UserProfile] WHERE Email = @Email";
            //see if this is the roleid needs to be changed
            bool bExists = false;

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Email", EMail);
                    int iRet = (int)Utils.ParseNumControlledReturn(DC.ExecuteScalar());
                    if (iRet > 0)
                        bExists = true;
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
            return bExists;
        }

        public User_Type FindUser(string IPAddress)
        {
            //use this to add a user for a 'remember me' option

            string sSQL = "SELECT u.[UserID], [UserName], [IPAddress], [FirstName], [LastName], [Email], r.RoleID, RoleName, RememberMe, m.IsConfirmed FROM [dbo].[UserProfile] u " +
                "LEFT JOIN [dbo].[webpages_Membership] m ON m.UserID = u.UserID " +
                "LEFT JOIN [dbo].[webpages_UsersInRoles] r ON r.UserID = u.UserID " +
                "LEFT JOIN [dbo].[webpages_Roles] wr ON wr.RoleID = r.RoleID " +
                "WHERE IPAddress = @IPAddress";

            User_Type user = new User_Type();

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("IPAddress", IPAddress);

                    DataTable dt = DC.ExecuteCommandForDT();

                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            int ID = (int)Utils.ParseNumControlledReturn(dr["UserID"]);
                            bool RememberMe = Utils.ParseBoolSafe(dr["RememberMe"]);
                            bool confirmed = Utils.ParseBoolSafe(dr["IsConfirmed"]);
                            if (RememberMe)
                                user = new User_Type
                                {
                                    Email = dr["Email"].StringSafe(),
                                    FirstName = dr["FirstName"].StringSafe(),
                                    IPAddress = IPAddress,
                                    LastName = dr["LastName"].StringSafe(),
                                    Pass = "RememberMe",
                                    RoleId = (int)Utils.ParseNumControlledReturn(dr["RoleID"]),
                                    RoleName = dr["RoleName"].StringSafe(),
                                    UserID = ID,
                                    UserName = dr["UserName"].StringSafe(),
                                    RememberMe = RememberMe,
                                    Confirmed = confirmed
                                };
                        }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("connection"))
                    {
                        //this is where we're going to find out that the database is inaccessible
                        DatabaseDown = true;
                    }
                    else
                    {
                        DC.MakeError(ex, Process, sSQL);

                    }
                }
                finally
                {
                    DC.Dispose();
                }

            }
            return user;
        }

        public string GetPasswordToken()
        {
            string sRet = "";
            string sSQL = "SELECT [ConfirmationToken] FROM [dbo].[webpages_Membership] WHERE [UserId]=@UserID";
            //see if this is the roleid needs to be changed
            User_Type user = new User_Type();
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    sRet = DC.ExecuteScalar().StringSafe();

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

        public User_Type GetUser(string UserName, string Password, string ipAddress, bool RememberMe)
        {
            string sSQL = "SELECT u.[UserID], [UserName], [IPAddress], [FirstName], [LastName], [Email], r.RoleID, RoleName, RememberMe, [IsConfirmed] FROM [dbo].[UserProfile] u " +
                "LEFT JOIN [dbo].[webpages_Membership] m ON m.UserID = u.UserID " +
                "LEFT JOIN [dbo].[webpages_UsersInRoles] r ON r.UserID = u.UserID " +
                "LEFT JOIN [dbo].[webpages_Roles] wr ON wr.RoleID = r.RoleID " +
                "WHERE u.[UserName] = @UserName AND (u.[Password] = @Password OR m.Password=@Password)";

            User_Type user = new User_Type();

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserName", UserName);
                    DC.AttachParameterByValue("Password", Password);

                    DataTable dt = DC.ExecuteCommandForDT();

                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            int ID = (int)Utils.ParseNumControlledReturn(dr["UserID"]);
                            bool remember = Utils.ParseBoolSafe(dr["RememberMe"]);
                            if (remember != RememberMe)
                                UpdateRemember(ID, RememberMe, DC);

                            user = new User_Type
                            {
                                Confirmed = Utils.ParseBoolSafe(dr["isConfirmed"]),
                                Email = dr["Email"].StringSafe(),
                                FirstName = dr["FirstName"].StringSafe(),
                                IPAddress = CompareIP(ID, dr["IPAddress"].StringSafe(), ipAddress, DC),
                                LastName = dr["LastName"].StringSafe(),
                                Pass = "",
                                RoleId = (int)Utils.ParseNumControlledReturn(dr["RoleID"]),
                                RoleName = dr["RoleName"].StringSafe(),
                                UserID = ID,
                                UserName = UserName,
                                RememberMe = RememberMe
                            };
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

            }
            return user;
        }

        public User_Type GetUserByIPAddress(string IPAddress)
        {
            User_Type user = new User_Type();

            using (Data DC = new Data("conn", Page, Process))
            {

                string sSQL = "SELECT IsConfirmed, prof.UserID, FirstName, LastName, UserName, Email, IPAddress, web.RoleID, RoleName, RememberMe FROM [dbo].[UserProfile] prof " +
                        "LEFT JOIN [dbo].[webpages_Membership] m on m.UserID = prof.UserID " +
                        "LEFT JOIN [dbo].[webpages_UsersInRoles] uir on uir.UserID = prof.UserID " +
                        "LEFT JOIN [dbo].[webpages_Roles] web on web.RoleId = uir.RoleId " +
                    "WHERE prof.IPAddress = @IPAddress";
                //see if this is the roleid needs to be changed

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("IPAddress", IPAddress);
                    DataTable dt = DC.ExecuteCommandForDT();
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            user = new User_Type
                            {
                                Confirmed = Utils.ParseBoolSafe(dr["IsConfirmed"]),
                                UserID = (int)Utils.ParseNumControlledReturn(dr["UserID"]),
                                FirstName = dr["FirstName"].StringSafe(),
                                LastName = dr["LastName"].StringSafe(),
                                UserName = dr["UserName"].StringSafe(),
                                Email = dr["Email"].StringSafe(),
                                IPAddress = dr["IPAddress"].StringSafe(),
                                RoleId = (int)Utils.ParseNumControlledReturn(dr["RoleID"]),
                                RoleName = dr["RoleName"].StringSafe(),
                                RememberMe = Utils.ParseBoolSafe(dr["RememberMe"])
                            };
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
            }
            return user;
        }

        public User_Type GetUserByUserID(int userID)
        {
            User_Type user = new User_Type();

                string sSQL = "SELECT * FROM [dbo].[UserProfile] prof " +
                        "LEFT JOIN [dbo].[webpages_Membership] m on m.UserID = prof.UserID " +
                        "LEFT JOIN [dbo].[webpages_UsersInRoles] uir on uir.UserID = prof.UserID " +
                        "LEFT JOIN [dbo].[webpages_Roles] web on web.RoleId = uir.RoleId " +
                    "WHERE prof.UserID = @UserID";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    DataTable dt = DC.ExecuteCommandForDT();
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            user = new User_Type
                            {
                                UserID = userID,
                                FirstName = dr["FirstName"].StringSafe(),
                                LastName = dr["LastName"].StringSafe(),
                                UserName = dr["UserName"].StringSafe(),
                                Email = dr["Email"].StringSafe(),
                                IPAddress = dr["IPAddress"].StringSafe(),
                                RoleId = (int)Utils.ParseNumControlledReturn(dr["RoleID"]),
                                RoleName = dr["RoleName"].StringSafe(),
                                RememberMe = Utils.ParseBoolSafe(dr["RememberMe"]),
                                Confirmed = Utils.ParseBoolSafe(dr["IsConfirmed"])
                            };
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
            }
            return user;
        }

        public int GetUserIDForPassword(string Email, string UserName)
        {
            int userID = -1;

                string sSQL = "SELECT UserID FROM [dbo].[UserProfile] WHERE email = @Email and username = @UserName";
            //see if this is the roleid needs to be changed
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Email", Email);
                    DC.AttachParameterByValue("UserName", UserName);
                    userID = (int) Utils.ParseNumControlledReturn(DC.ExecuteScalar());
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
            return userID;
        }
        public bool LogIn()
        {
            string sSQL = "UPDATE [dbo].[UserProfile] SET [LastLogIn] = getdate() WHERE UserID = @UserID";
            //see if this is the roleid needs to be changed
            bool bRet = false;

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    DC.ExecuteCommand();
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
        public bool LogOut()
        {
            string sSQL = "UPDATE [dbo].[UserProfile] SET [LastLogIn] = getdate(), [RememberMe]=0 WHERE UserID = @UserID";
            //see if this is the roleid needs to be changed
            bool bRet = false;
            using (Data DC = new Data("conn", Page, Process))
            {


                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    DC.ExecuteCommand();
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

        public bool SaveNewToken(int UserID, string sToken)
        {
            bool bRet = false;
            string sSQL = "UPDATE [dbo].[webpages_Membership] SET [ConfirmationToken] = @Token WHERE [UserId]=@UserID";
            //see if this is the roleid needs to be changed
            User_Type user = new User_Type();

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Token", sToken);
                    DC.AttachParameterByValue("UserID", UserID);
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

        public bool SavePasswordToken(int UserID, string sToken)
        {
            bool bRet = false;
            string sSQL = "UPDATE [dbo].[webpages_Membership] SET [ConfirmationToken] = @Token WHERE [UserId]=@UserID";
            //see if this is the roleid needs to be changed
            User_Type user = new User_Type();

            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Token", sToken);
                    DC.AttachParameterByValue("UserID", UserID);
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

        public bool SetNewPassword(string UserPassword)
        {
            bool bRet = false;
            string sSQL = "UPDATE [dbo].[webpages_Membership] SET [Password] = @Password, [PasswordChangedDate] = getdate() WHERE [UserId]=@UserID";
            //see if this is the roleid needs to be changed
            User_Type user = new User_Type();
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Password", UserPassword);
                    DC.AttachParameterByValue("UserID", UserID);
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
        public User_Type VerifyUserByEmail(string EMail)
        {
            string sSQL = "SELECT * FROM [dbo].[UserProfile] prof " +
                        "LEFT JOIN [dbo].[webpages_UsersInRoles] uir on uir.UserID = prof.UserID " +
                        "LEFT JOIN [dbo].[webpages_Roles] web on web.RoleId = uir.RoleId " +
                    "WHERE prof.Email = @Email";
            //see if this is the roleid needs to be changed
            User_Type user = new User_Type();
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("Email", EMail);
                    DataTable dt = DC.ExecuteCommandForDT();
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[0];
                            user = new User_Type
                            {
                                UserID = (int)Utils.ParseNumControlledReturn(dr["UserID"]),
                                FirstName = dr["FirstName"].StringSafe(),
                                LastName = dr["LastName"].StringSafe(),
                                UserName = dr["UserName"].StringSafe(),
                                Email = dr["Email"].StringSafe(),
                                IPAddress = dr["IPAddress"].StringSafe(),
                                RoleId = (int)Utils.ParseNumControlledReturn(dr["RoleID"]),
                                RoleName = dr["RoleName"].StringSafe()
                            };
                        }
                    //now register this person
                    sSQL = "UPDATE [dbo].[webpages_Membership] SET [IsConfirmed] =1 WHERE UserID = " + user.UserID;
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.ExecuteCommand();

                    sSQL = "UPDATE [dbo].[webpages_UsersInRoles] SET RoleID =2 WHERE UserID = " + user.UserID;
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.ExecuteCommand();

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
            return user;
        }

        private string CompareIP(int UserID, string existing, string incoming, Data DC)
        {

            if (existing != incoming)
            {
                string sSQL = "UPDATE [dbo].[UserProfile] SET IPAddress = @IPAddress WHERE UserID = @UserID";
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    DC.AttachParameterByValue("IPAddress", incoming);

                    DC.ExecuteCommand();
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
            return incoming;
        }
        private void UpdateRemember(int ID, bool remember, Data DC)
        {
            string sSQL = "UPDATE [dbo].[UserProfile] SET [RememberMe] = @RememberMe WHERE UserID = @UserID";

            try
            {
                DC.AddCommand(CommandType.Text, sSQL);
                DC.AttachParameterByValue("UserID", ID);
                DC.AttachParameterByValue("RememberMe", remember);
                DC.ExecuteCommand();
            }
            catch (Exception ex)
            {
                DC.MakeError(ex, Process, sSQL);

            }

        }
        public bool UpdateUser(User_Type user)
        {
            bool bRet = false;

            string sSQL = "Update [dbo].[UserProfile] SET [FirstName] = @FirstName, [LastName] = @LastName, [Email] = @Email, " +
                "[LastLogin] = getdate(), [UserName] = @UserName WHERE UserID = @UserID";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("FirstName", user.FirstName.StringSafe());
                    DC.AttachParameterByValue("LastName", user.LastName.StringSafe());
                    DC.AttachParameterByValue("Email", user.Email.StringSafe());
                    DC.AttachParameterByValue("UserName", user.UserName.StringSafe());
                    DC.AttachParameterByValue("UserID", UserID);
                    int iRet = DC.ExecuteCommand();

                    if (iRet > 0)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
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
            }
            return bRet;
        }
        public bool VerifyUser(int UserID, string UserToken)
        {
            bool bRet = false;

            string sSQL = "SELECT count(u.UserID) FROM [dbo].[UserProfile] u " +
                    "LEFT JOIN [dbo].[webpages_Membership] m on m.UserID = u.UserID " +
                    "WHERE u.UserID = @UserID AND m.ConfirmationToken =@Token";
            using (Data DC = new Data("conn", Page, Process))
            {
                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserID", UserID);
                    DC.AttachParameterByValue("Token", UserToken.Replace(" ", ""));
                    object obj = DC.ExecuteScalar();
                    int iRet = (int)Utils.ParseNumControlledReturn(obj);

                    if (iRet > 0)
                    {
                        sSQL = "UPDATE [dbo].[webpages_Membership] SET [IsConfirmed] = 1 WHERE UserID = @UserID";
                        DC.AddCommand(CommandType.Text, sSQL);
                        DC.AttachParameterByValue("UserID", UserID);
                        DC.ExecuteCommand();
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
            }
            return bRet;
        }

        private bool VerifyUser(string UserName, string EMail)
        {
            bool bRet = false;

            string sSQL = "SELECT count(userID) FROM [dbo].[UserProfile] WHERE (upper([UserName]) = upper(@UserName)) OR (upper([Email]) = upper(@Email));";
            using (Data DC = new Data("conn", Page, Process))
            {

                try
                {
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("UserName", UserName);
                    DC.AttachParameterByValue("Email", EMail);
                    object obj = DC.ExecuteScalar();
                    int iRet = (int)Utils.ParseNumControlledReturn(obj);

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
