using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier.Types
{
    public class Blog_Type
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
        public List<Blog_Type> SubCategories { get; set; }
    }
}
