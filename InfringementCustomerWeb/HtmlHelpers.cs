using System;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace InfringementCustomerWeb.Helpers
{
    public static class HtmlHelpers
    {
        public static string SectionLink(this HtmlHelper html, string URL, string display)
        {
            return String.Format("<a href=\"{0}\">{1}</a>", URL, display);
        }

        public static bool ValidateEmail(string email)
        {
            bool rvalue = true;

            if (email != null && email.Trim().Length > 0)
            {
                if (email.Contains(","))
                {
                    string[] emaillist = email.Split(',');
                    foreach (string e in emaillist)
                    {
                        if (e != null && e.Trim() != "")
                            if (!Regex.IsMatch(e.Trim(), @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                            {
                                rvalue = false;
                                break;
                            }
                    }
                }
                else
                    rvalue = Regex.IsMatch(email.Trim(), @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            }
            else
                rvalue = false;


            return rvalue;
        }
    }
}