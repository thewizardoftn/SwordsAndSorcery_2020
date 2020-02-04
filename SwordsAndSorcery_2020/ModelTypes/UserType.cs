using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwordsAndSorcery_2020.ModelTypes
{
    public class UserType
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string IPAddress { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime SessionStart { get; set; }
        public bool Pass { get; set; }
        public bool RememberMe { get; set; }
        public string Token { get; set; }
        public bool Confirmed { get; set; }
        public bool DownArrow { get; set; }
        public DateTime CreateDate { get; set; }
    }
}