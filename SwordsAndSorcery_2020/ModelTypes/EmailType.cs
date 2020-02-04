using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class EmailType
    {
        public int ID { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public DateTime SentTime { get; set; }
        public string Message { get; set; }
        public string Combined
        {
            get
            {
                return SenderName + "<" + SenderEmail + ">";

            }
        }
    }
}