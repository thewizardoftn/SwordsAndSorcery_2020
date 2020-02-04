using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier.Types
{
    public class UserSpec
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
