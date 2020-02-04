using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.Models;
using Utilities;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class NewsCollection
    {
        public int ID { get; set; }
        public DateTime NewsDate { get; set; }
        public string NewsDateString { get { return NewsDate.RemoveMinDate(); } }
        public string NewsItem { get; set; }
    }
}