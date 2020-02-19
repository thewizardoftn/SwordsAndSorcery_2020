using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Utilities
{
    public static class Utils
    {
        public static decimal dfault { get; set; }
        public static DateTime? datefault { get; set; }

        public static string BuildName(string FirstName, string LastName, string MiddleName, string Suffix)
        {
            string fullName = "";
            if (FirstName.StringSafe().Length > 0)
                fullName = FirstName.StringSafe() + " ";
            if (MiddleName.StringSafe().Length > 0)
                fullName += MiddleName.StringSafe().Substring(0, 1) + ". ";
            if (LastName.StringSafe().Length > 0)
                fullName += LastName.StringSafe() + " ";
            if (Suffix.StringSafe().Length > 0)
                fullName += ", " + Suffix.StringSafe();
            return fullName;
        }
        public static byte[] ByteSafe(string data)
        {
            byte[] outData = new byte[0];
            try
            {
                outData = Array.ConvertAll(data.Split('-'), s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
            }
            catch { }
            return outData;
        }

        public static BreakOutType EmailBreakOut(string email)
        {
            string firstName = "";
            string lastName = "";
            if (email.Contains("<"))
            {
                //there is an associated name
                //  format FirstName MiddleInitial. LastName, Suffix
                string name = email.Substring(0, email.IndexOf("<"));
                if (name.Contains(" "))
                {
                    //there is a first and a last name
                    string[] rename = name.Split(' ');
                    switch (rename.Length)
                    {
                        case 2:
                            firstName = rename[0];
                            lastName = rename[1];
                            break;
                        case 3:                         //assumes a middle name
                            firstName = rename[0];
                            lastName = rename[2];
                            break;
                        case 4:                         //assumes a suffix, which we just ignore
                            firstName = rename[0];
                            lastName = rename[2];
                            break;
                        default:
                            lastName = name;
                            break;
                    }
                }
                else
                    lastName = name;
                //now trim down the email
                email = email.Substring(email.IndexOf("<") + 1, (email.Length - (email.IndexOf("<") + 1) - 1));
            }
            BreakOutType type = new BreakOutType
            {
                Address = email,
                LastName = lastName,
                FirstName = firstName
            };

            return type;
        }


        public static bool IsDate(object value)
        {
            bool bRet = false;
            if (value != null)
            {
                if (value.ToString().Length > 0)
                {
                    try
                    {
                        DateTime dDate = DateTime.MinValue;
                        if (DateTime.TryParse(value.ToString(), out dDate))
                        {
                            if (dDate > DateTime.MinValue)
                                bRet = true;
                        }

                    }
                    catch { }
                }
            }
            return bRet;
        }
        public static bool IsEmail(object value)
        {
            bool bRet = false;
            string sPattern = @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b";
            if (value != null)
                if (Regex.IsMatch(value.ToString(), sPattern, RegexOptions.IgnoreCase))
                    bRet = true;

            return bRet;
        }

        public static bool IsNumeric(object value)
        {
            bool bRet = false;
            decimal i = 0;
            if (value == null)
                bRet = false;
            else if (value.ToString() == "0")
                bRet = true;
            else if (decimal.TryParse(value.ToString(), out i))
            {
                if (i > 0 || (i * -1) > 0)
                    bRet = true;
            }

            return bRet;
        }
        public static string MessageImages(string notes, Dictionary<string, byte[]> files)  //1 = email, 2 = history, 3 = pending
        {
            do
            {
                int start = notes.IndexOf("[image:");
                int end = notes.Substring(start, notes.Length - start).IndexOf(']');
                //now we have the position of the item - get it, and parse out the file name
                string caption = notes.Substring(notes.IndexOf("[image:"), (notes.IndexOf(']') + 1) - notes.IndexOf("[image:"));
                string imageName = caption.Substring(caption.IndexOf(':') + 1, (caption.IndexOf(']') - (caption.IndexOf(':') + 1))).Trim();

                //get the file
                byte[] data = new byte[0];
                if (files.TryGetValue(imageName, out data))
                {
                    string imageBase64 = Convert.ToBase64String(data);
                    string imageSrc = "<img src='" + string.Format("data:image/gif;base64,{0}", imageBase64) + "' alt='" + imageName + "', title='" + imageName + "' class='emailImageClass' />";
                    notes = notes.Replace(caption, imageSrc);
                }

            } while (notes.IndexOf("[images:") > -1);
            return notes;
        }
        public static bool ParseNumSafe(object value, out decimal retVal)
        {
            if (value == null)
            {
                retVal = 0;
                return false;
            }
            else if (value.ToString().Contains("E+"))
            {
                //this contains scientific notation
                decimal dRet = 0;
                try
                {
                    dRet = Decimal.Parse(value.ToString(), System.Globalization.NumberStyles.Float);
                    retVal = dRet;
                    return true;
                }
                catch
                {
                    retVal = dRet;
                    return false;
                }
            }
            else
                return decimal.TryParse(value.ToString(), out retVal);
        }
        public static decimal ParseNumControlledReturn(object value)                    //this returns a controlled return in a single line
        {
            decimal d = 0;
            if (ParseNumSafe(value, out d))
                return d;
            else
                return dfault;
        }
        public static bool ParseDateSafe(object value, out DateTime d)
        {
            return DateTime.TryParse(value.ToString(), out d);
        }
        public static DateTime ParseDateControlledReturn(object value)
        {
            DateTime d = DateTime.Today;
            if (ParseDateSafe(value, out d))
                return d;
            else
            {
                if (datefault == null)
                    datefault = d;
                return datefault.Value;
            }
        }
        public static string FormatNegativeValue(decimal dIn)
        {
            string sRet = string.Format("{0:###,###,###.##}", dIn);
            if (dIn < 0)
                sRet = string.Format("{0:(###,###,###.##)}", 0 - dIn);
            return sRet;
        }
        public static string FormatNegativeDollarValue(decimal dIn)
        {
            string sRet = string.Format("{0:$###,###,###.##}", dIn);
            if (dIn < 0)
                sRet = string.Format("{0:($###,###,###.##)}", 0 - dIn);
            return sRet;
        }
        public static bool ParseBoolSafe(object value)
        {
            bool bRet = false;
            int iRet = 0;

            try
            {
                if (!bool.TryParse(value.ToString(), out bRet))
                {
                    if (int.TryParse(value.ToString(), out iRet))
                    {
                        if (Math.Abs(iRet) == 1)
                            bRet = true;
                    }
                    else                                            //the value is a string
                    {
                        if (value.ToString().ToLower() == "true")
                            bRet = true;
                        else if (value.ToString().ToLower() == "t" || value.ToString().ToLower() == "y")
                            bRet = true;
                    }
                }
            }
            catch { }

            return bRet;
        }
        public static string ProcessResult(int i)
        {
            string sRet = "Success";
            switch (i)
            {
                case -3:
                    sRet = "Secondary data insert error";
                    break;
                case -2:
                    sRet = "Database Error";
                    break;
                case -1:
                    sRet = "Error";
                    break;
                case 0:
                    sRet = "Failure - reason unknown";
                    break;

            }

            return sRet;
        }
    }
}
