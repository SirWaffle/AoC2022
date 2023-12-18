using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{
    public static class StringExt
    {
        public static string AppendFile(this string value, string filename)
        {
            return value + File.ReadAllText(filename);
        }

        public static string ConvertWhitespacesToSingleSpaces(this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }

        public static IEnumerable<string> Lines(this string value, string term = "\n")
        {
            return value.Split(term);
        }

        public static IEnumerable<string> Words(this string value, bool condenseWhitespace = true, string seperator = " ")
        {
            if (condenseWhitespace)
                return value.ConvertWhitespacesToSingleSpaces().Split(seperator);

            return value.Split(seperator);
        }

        public static long AsInt64(this string value)
        {
            return Convert.ToInt64(value);
        }
    }
}
