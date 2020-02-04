using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTier.Types
{
    public class PageHitType
    {
        public DateTime HitDate { get; set; }
        public String PageName { get; set; }
        public int Hits { get; set; }
    }
}
