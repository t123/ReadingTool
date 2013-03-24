using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadingTool.Common
{
    public class Tags
    {
        public const string SEPARATOR = @" ";

        /// <summary>
        /// Changes a list of strings to a single string separated by SEPARATOR
        /// </summary>
        /// <param name="input">A list of tags</param>
        /// <returns>A string of tags separated by SEPARATOR</returns>
        public static string ToString(IEnumerable<string> input)
        {
            if(input == null) return "";
            return string.Join(SEPARATOR, input.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x));
        }

        /// <summary>
        /// Changes a single string separated by SEPARATOR to an array of string[]
        /// </summary>
        /// <param name="input">A string of tags separated by SEPARATOR</param>
        /// <returns>A string[] of the tags</returns>
        public static string[] ToTags(string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return new string[] { };

            IList<string> list = input.ToLowerInvariant().Split(new string[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            return list.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// Merges an existing string[] and a single string separated by SEPARATOR to an array of string[]
        /// </summary>
        /// <param name="original">A list of strings</param>
        /// <param name="input">A string of tags separated by SEPARATOR</param>
        /// <returns>A string[] of the tags</returns>
        public static string[] ToTags(IEnumerable<string> original, string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return original.ToArray();

            List<string> list = new List<string>(original);
            list.AddRange(input.ToLowerInvariant().Split(new string[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries));
            return list.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x).ToArray();
        }
    }
}