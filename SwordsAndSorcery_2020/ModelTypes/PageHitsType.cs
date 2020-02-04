using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class PageHitsType
    {
        public DateTime HitDate { get; set; }
        public String PageName { get; set; }
        public int Hits { get; set; }
    }
}