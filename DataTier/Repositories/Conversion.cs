using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTier.Repository
{
    public static class Conversion
    {
        //coming out of the database
        public static string ConvertOut(string content)
        {
            string outText = content.Replace("[", "<");
            outText = outText.Replace("]", ">");

            return outText;
        }
        //going into the database
        public static string ConvertIn(string content)
        {
            string outText = content.Replace("<", "[");
            outText = outText.Replace(">", "]");

            return outText;
        }
    }
}
