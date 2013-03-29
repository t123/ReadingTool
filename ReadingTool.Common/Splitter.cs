#region License
// Splitter.cs is part of ReadingTool.Common
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReadingTool.Common
{
    public class Splitter
    {
        private static readonly Regex DEFAULT_PATTERN = new Regex("\\s+", RegexOptions.Compiled);
        private readonly Regex _pattern;
        private readonly bool _keepDelimiters;

        public Splitter(Regex pattern, bool keepDelimiters)
        {
            _pattern = pattern;
            _keepDelimiters = keepDelimiters;
        }

        public Splitter(string pattern, bool keepDelimiters)
            : this(string.IsNullOrWhiteSpace(pattern) ? DEFAULT_PATTERN : new Regex(pattern, RegexOptions.Compiled), keepDelimiters)
        {
        }

        //public Splitter(Regex pattern)
        //    : this(pattern, true)
        //{
        //}

        //public Splitter(string pattern) : this(pattern, true) { }
        //public Splitter(bool keepDelimiters) : this(DEFAULT_PATTERN, keepDelimiters) { }
        //public Splitter() : this(DEFAULT_PATTERN) { }

        public string[] Split(string text)
        {
            if(string.IsNullOrEmpty(text))
                text = "";

            int lastMatch = 0;
            IList<string> splitted = new List<string>();

            MatchCollection m = _pattern.Matches(text);

            foreach(Match match in m)
            {
                string substring = text.Substring(lastMatch, match.Index - lastMatch);
                splitted.Add(substring);

                if(_keepDelimiters)
                {
                    foreach(Group g in match.Groups)
                    {
                        if(string.IsNullOrEmpty(g.Value))
                            continue;

                        splitted.Add(g.Value);
                        break;
                    }
                }

                lastMatch = match.Index + match.Length;
            }

            if(lastMatch < text.Length)
            {
                splitted.Add(text.Substring(lastMatch, text.Length - lastMatch));
            }

            return splitted.ToArray();
        }
    }
}
