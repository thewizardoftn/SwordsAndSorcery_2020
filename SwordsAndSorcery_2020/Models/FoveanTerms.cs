using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;
using DataTier.Repository;
using DataTier.Types;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class FoveanTerms
    {
        public List<FoveaType> Terms { get; set; }
        public int UserID { get; set; }

        public FoveanTerms()
        {
            UserType user = UserCache.GetFromCache(0, Utils.GetIPAddress());
            if (user == null)
                return;                     //this is an indicator that the database is down

            UserID = user.UserID;

            FoveaRepository rep= new FoveaRepository(UserID, "FoveanTerms", "FoveanTerms");
            rep.FoveaTerms();
            List<FoveaTerms_Type> items  = rep.Terms;
            List<FoveaType> terms = new List<FoveaType>();

            foreach (var item in items)
            {
                terms.Add(new FoveaType
                {
                    Approved = item.Approved,
                    Description = item.Description,
                    FoveaID = item.FoveaID,
                    Subject = item.Subject
                });
            }
            Terms = terms;
        }
        public bool AddApprovedTerm(string Subject, string Description)
        {
            FoveaRepository rep = new FoveaRepository(UserID, "FoveanTerms", "AddApprovedTerm");
            bool bRet = rep.AddApprovedTerm(Subject, Description);

            return bRet;
        }
        public bool AddTerm(string Subject, string Description)
        {
            FoveaRepository rep = new FoveaRepository(UserID, "FoveanTerms", "AddTerm");
            bool bRet = rep.AddTerm(Subject, Description);

            return bRet;
        }
        public bool RemoveTerm(int FoveaID)
        {
            FoveaRepository rep = new FoveaRepository(UserID, "FoveanTerms", "RemoveTerm");
            return rep.RemoveTerm(FoveaID);
        }
        public bool UpdateTerm(string Subject, string Description, int FoveaID)
        {
            return new FoveaRepository(UserID, "FoveanTerms", "UpdateTerm").UpdateTerm(FoveaID, Subject, Description);
        }
    }
}