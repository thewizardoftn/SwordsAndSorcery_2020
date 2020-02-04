using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using DataTier.Repository;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class FeedbackModel
    {
        [Required]
        [MinLength(6)]
        public string SenderName { get; set; }
        [Required]
        [MinLength(6)]
        public string SenderEmail { get; set; }
        [Required]
        [MinLength(10)]
        public string Message { get; set; }
        public string Error { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Solution { get; set; }
        public int UserID { get; set; }

        public FeedbackModel()
        {
            UserType user = UserCache.GetFromCache(0, Utils.GetIPAddress());
            if (user == null)
                return;                     //this is an indicator that the database is down

            UserID = user.UserID;
        }

        public bool SaveFeedback(string IP)
        {
            bool bRet = false;
            if (Message == null || SenderEmail == null || SenderName == null)
                Error = "You must enter a name, email address and a message";
            else
            {
                FeedbackRepository f = new FeedbackRepository(UserID, "FeedbackModel", "SaveFeedback");
                f.Feedback(SenderName, SenderEmail, Message, IP);
                if (f.O.ToString() != "-1")
                    bRet = true;
            }
            return bRet;
        }
    }
}