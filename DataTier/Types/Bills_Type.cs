using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Types
{
    public class Bills_Type
    {
        public int UserID { get; set; }
        public int BillsID { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Sched { get; set; }
    }
}
