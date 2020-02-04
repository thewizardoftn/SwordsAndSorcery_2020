using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataTier.Types;
using DataTier;
using SwordsAndSorcery_2020.ModelTypes;

namespace SwordsAndSorcery_2020.Models
{
    public class SaveError
    {
        public string Error { get; set; }
        public string Subject { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public int UserID { get; set; }
        public string Literal { get; set; }
        public string sSQL { get; set; }

        public SaveError()
        {

        }

        public List<ErrorType> FindErrors(bool archive, int UserID)
        {

            List<Error_Type> errors = Errors.ReturnErrors(archive, UserID);
            List<ErrorType> err = new List<ErrorType>();

            foreach (var item in errors)
            {
                err.Add(new ErrorType
                {
                    UserID = item.UserID,
                    Err_Message = item.Err_Message,
                    Err_Subject = item.Err_Subject,
                    ErrorID = item.ErrorID,
                    LiteralDesc = item.LiteralDesc,
                    Page = item.Page,
                    Process = item.Process,
                    SQL = item.SQL,
                    Time_Of_Error = item.Time_Of_Error
                });
            }

            return err;
        }
        public void ReportError(ErrorType error)
        {
            Error_Type type = new Error_Type
            {
                UserID = error.UserID,
                Err_Message = error.Err_Message,
                Err_Subject = error.Err_Subject,
                ErrorID = error.ErrorID,
                LiteralDesc = error.LiteralDesc,
                Page = error.Page,
                Process = error.Process,
                SQL = error.SQL,
                Time_Of_Error = error.Time_Of_Error
            };

        }
        public bool UpdateError(int id, int UserID, string Desc)
        {
            return Errors.UpdateError(id, UserID, Desc);
        }
    }
}