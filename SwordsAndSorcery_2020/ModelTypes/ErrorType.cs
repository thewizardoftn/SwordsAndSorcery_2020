using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class ErrorType
    {
        public string Err_Message { get; set; }
        public string Err_Subject { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public DateTime Time_Of_Error { get; set; }
        public int UserID { get; set; }
        public string LiteralDesc { get; set; }
        public string SQL { get; set; }
        public int ErrorID { get; set; }
    }
}