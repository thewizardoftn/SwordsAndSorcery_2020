using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class HeaderModel
    {
        public UserType User { get; set; }

        public HeaderModel()
        {
            User = UserCache.GetFromCache(0, Utils.GetIPAddress());
        }
    }
}