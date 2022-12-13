using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker
{
    public static class Utils
    {
        public static int MonthNumber(string monthShortName)
        {
            switch(monthShortName)
            {
                case "Jan": return 1;
                case "Feb": return 2;
                case "Mar": return 3;
                case "Apr": return 4;
                case "May": return 5;
                case "Jun": return 6;
                case "Jul": return 7;
                case "Aug": return 8;
                case "Sep": return 9;
                case "Oct": return 10;
                case "Nov": return 11;
                case "Dec": return 12;
            }
            return 1;
        }
        public static bool ParseDateFromGitLog(string dateInLog, out DateTime date)
        {
            date = DateTime.MinValue;

            //Mon Feb 22 13:20:34 2021 +0100
            string pattern = "(\\w{3}) (\\w{3}) (\\d{1,2}) (\\d{1,2}):(\\d{2}):(\\d{2}) (\\d{4})";
            Match match = Regex.Match(dateInLog, pattern);
            if (match.Success)
            {
                int month = MonthNumber(match.Groups[2].Value);
                if (int.TryParse(match.Groups[3].Value, out int day) &&
                    int.TryParse(match.Groups[4].Value, out int hour) &&
                    int.TryParse(match.Groups[5].Value, out int minute) &&
                    int.TryParse(match.Groups[6].Value, out int second) &&
                    int.TryParse(match.Groups[7].Value, out int year))
                {
                    date = new DateTime(year, month, day, hour, minute, second);
                    return true;
                }
            }
            return false;
        }

        public static string DoubleToString(double value, int numDecimals)
        {
            string output = value.ToString($"F{numDecimals}", CultureInfo.InvariantCulture);
            return output;
        }
    }
}
