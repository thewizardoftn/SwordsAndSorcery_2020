using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SwordsAndSorcery_2020.ModelTypes;

namespace SwordsAndSorcery_2020.Models
{
    public class FoveaModel
    {
        public List<FoveaType> Terms { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }

        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Solution { get; set; }
        public string Message { get; set; }
        public FoveaModel()
        {
            Terms = new FoveanTerms().Terms;
        }
        public bool AddTerm()
        {
            bool bRet = new FoveanTerms().AddTerm(Subject, Description);
            return bRet;
        }
    }
}