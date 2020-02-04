using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class BlogType
    {
        public int BlogID { get; set; }
        public int ParentID { get; set; }
        public string Title { get; set; }
        public DateTime BlogDate { get; set; }
        public int Author { get; set; }
        public string AuthorName { get; set; }
        public string BlogContent { get; set; }
        public int BlogTrailID { get; set; }
        public bool Sticky { get; set; }
        public bool Archive { get; set; }
        public bool Banned { get; set; }
        public List<BlogType> SubCategories { get; set; }
    }
}