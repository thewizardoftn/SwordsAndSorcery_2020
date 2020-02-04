using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTier.Types
{
    public class Error_Type
    {
        private string userName = "";

        public string Err_Message { get; set; }
        public string Err_Subject { get; set; }
        public string Page { get; set; }
        public string Process { get; set; }
        public DateTime Time_Of_Error { get; set; }
        public int UserID { get; set; }
        public string UserName
        {
            get
            {
                if (userName.Length == 0)
                    userName = "unknown";
                return userName;
            }
            set { userName = value; }
        }
        public string LiteralDesc { get; set; }
        public string SQL { get; set; }
        public int ErrorID { get; set; }
        public bool Corrected { get; set; }
    }
}
