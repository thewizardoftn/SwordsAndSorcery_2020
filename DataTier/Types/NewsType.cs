using System;
using Utilities;

namespace DataTier.Types
{
    public class NewsType
    {
        public int ID { get; set; }
        public DateTime NewsDate { get; set; }
        public string NewsDateString { get { return NewsDate.RemoveMinDate(); } }
        public string NewsItem { get; set; }
    }
}
