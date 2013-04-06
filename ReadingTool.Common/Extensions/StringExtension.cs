using System;

namespace ReadingTool.Common.Extensions
{
    public static class StringExtension
    {
        public static string ToBase64(this String theString)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(theString);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string NewlineToBreak(this String theString)
        {
            if(string.IsNullOrEmpty(theString)) return theString;

            return theString.Replace("\n", "<br/>");
        }

        public static string Replace(this string source, string oldString, string newString, StringComparison comp)
        {
            if(string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldString))
            {
                return source;
            }

            if(newString == null)
            {
                newString = "";
            }

            int index = source.IndexOf(oldString, comp);
            if(index >= 0)
            {
                source = source.Remove(index, oldString.Length);
                source = source.Insert(index, newString);
            }

            return source;
        }
    }
}
