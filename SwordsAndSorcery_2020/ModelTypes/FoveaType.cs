using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class FoveaType
    {
        public int FoveaID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public bool Approved { get; set; }
    }
}