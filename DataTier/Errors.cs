using System;
using System.Collections.Generic;
using System.Data;
using DataTier.Types;
using System.Data.SqlClient;
using Utilities;

namespace DataTier
{
    public class Errors : IErrorHandler
    {
        public string Result { get; set; }
        public string ConnectionString { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public Errors(string connectionString, string source)
        {
            ConnectionString = connectionString;
            Source = source;
        }
        public void LogError(Exception ex, int UserID, string Page, string Process, string Subject, string SQL)
        {
            //don't log errors while developing
            if (!Source.ToLower().Contains("localhost:"))
            {
                //our first instinct is to record an error to the database
                string sSQL = "";
                //Invalid column name 'UserID'.\r\nInvalid column name 'TimeOfError'.\r\nInvalid column name 'Corrected'.
                sSQL = "Insert Into dbo.ErrorTable " +
                         "(UserID, Page, Process, Err_Message, Err_Subject, LiteralDesc, [Time_Of_Error], SQL, Corrected)" +
                         " VALUES (@UserID, @Page, @Process, @Err_Message, @Err_Subject, @LiteralDesc, getdate(), @SQL, 0)";

                SqlConnection oConn = new SqlConnection(ConnectionString);
                try
                {
                    //make an independent connection to the error database
                    oConn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = oConn;
                    string sMess = ex.Message.ToString();
                    if (sMess.Length > 150)
                        sMess = sMess.Substring(0, 149);

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sSQL;
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    cmd.Parameters.AddWithValue("Page", Page.StringSafe());
                    cmd.Parameters.AddWithValue("Process", Process.StringSafe());
                    cmd.Parameters.AddWithValue("Err_Message", sMess.StringSafe());
                    cmd.Parameters.AddWithValue("Err_Subject", Subject.StringSafe());
                    cmd.Parameters.AddWithValue("LiteralDesc", ex.StackTrace.StringSafe());
                    cmd.Parameters.AddWithValue("SQL", SQL.StringSafe());

                    int iRet = cmd.ExecuteNonQuery();

                    if (iRet != 1)
                    {
                        Result = "Error writing to the error database";

                    }
                    else
                        Result = "Error recorded";
                }
                catch (Exception ex2)
                {
                    Result = ex2.Message;
                }
                finally
                {
                    if (oConn.State == ConnectionState.Open)
                        oConn.Close();
                    oConn.Dispose();
                }
            }
        }
        public static List<Error_Type> ReturnErrors(bool corrected, int UserID)
        {
            List<Error_Type> errors = new List<Error_Type>();
            //our first instinct is to record an error to the database
            string sSQL = "SELECT * FROM dbo.ErrorTable ORDER BY ErrorID DESC";

            using (Data DC = new Data("conn", "Errors", "ReturnErrors"))
            {
                try
                {
                    //make an independent connection to the error database
                    DC.AddCommand(CommandType.Text, sSQL);

                    DataTable dt = DC.ExecuteCommandForDT();

                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {

                            errors.Add(new Error_Type
                            {
                                UserID = UserID,
                                Err_Message = dr["Err_Message"].StringSafe(),
                                Err_Subject = dr["Err_Subject"].StringSafe(),
                                ErrorID = (int)Utils.ParseNumControlledReturn(dr["ErrorID"]),
                                LiteralDesc = dr["LiteralDesc"].StringSafe(),
                                Page = dr["Page"].StringSafe(),
                                Process = dr["Process"].StringSafe(),
                                SQL = dr["SQL"].StringSafe(),
                                Time_Of_Error = Utils.ParseDateControlledReturn(dr["Time_Of_Error"])

                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    DC.MakeError(ex, "ReturnErrors", sSQL);
                }
                finally
                {
                    if (DC != null)
                        DC.Dispose();
                }
            }
            return errors;
        }
        public static bool UpdateError(int id, int UserID, string Desc)
        {
            bool bRet = false;
            string sSQL = "";
            sSQL = "UPDATE dbo.ErrorTable SET Corrected = 1 WHERE ErrorID=@ID;";

            using (Data DC = new Data("conn", "Errors", "UpdateError"))
            {
                try
                {
                    //make an independent connection to the error database
                    DC.AddCommand(CommandType.Text, sSQL);
                    DC.AttachParameterByValue("@UserID", UserID);
                    DC.AttachParameterByValue("Desc", Desc);
                    DC.AttachParameterByValue("ID", id);

                    int iRet = DC.ExecuteCommand();

                    if (iRet > 0)
                        bRet = true;
                }
                catch
                {
                    bRet = false;
                }
                finally
                {
                    if (DC != null)
                        DC.Dispose();
                }
            }
            return bRet;
        }
    }
}
