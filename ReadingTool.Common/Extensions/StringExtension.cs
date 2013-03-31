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
    }
}
