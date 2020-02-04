using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier.Types
{
    public class EmailDataType
    {
        public int ID { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public DateTime SentTime { get; set; }
        public string Message { get; set; }
    }
}
