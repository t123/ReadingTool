using System.Text.RegularExpressions;

namespace ReadingTool.Core
{
    public static class String
    {
        /// <summary>
        /// http://predicatet.blogspot.com/2009/04/improved-c-slug-generator-or-how-to.html
        /// </summary>
        /// <param name="theString"></param>
        /// <returns></returns>
        public static string Slugify(this string theString)
        {
            string str = theString.RemoveAccent().ToLower().Trim();

            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // invalid chars           
            str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space   
            //str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim(); // cut and trim it   
            str = Regex.Replace(str, @"\s", "-"); // hyphens   

            return str;
        }

        public static string RemoveAccent(this string theString)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(theString);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string CamelCaseToWords(this string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}
