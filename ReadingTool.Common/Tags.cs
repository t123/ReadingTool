#region License
// Tags.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

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