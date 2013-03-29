#region License
// SearchFilterParser.cs is part of ReadingTool.Common
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

namespace ReadingTool.Common.Search
{
    public class SearchFilterParser
    {
        public class FilterModel
        {
            public IList<string> Reserved { get; set; }
            public IList<string> Tags { get; set; }
            public IList<string> Other { get; set; }

            public FilterModel()
            {
                Tags = new List<string>();
                Other = new List<string>();
                Reserved = new List<string>();
            }
        }

        private static readonly Regex regex = new Regex(@"#[\w]+|\w+|""[a-zA-ZÀ-ÖØ-öø-ȳ\-\'\s]*""");

        public static FilterModel Parse(string filter, string[] reservedTerms = null)
        {
            if(reservedTerms == null)
            {
                reservedTerms = new string[0];
            }

            filter = (filter ?? "").ToLowerInvariant().Trim();
            FilterModel model = new FilterModel();

            foreach(Match s in regex.Matches(filter))
            {
                var t = s.Value;

                if(t.StartsWith("\"")) t = t.Substring(1, t.Length - 1);
                if(t.EndsWith("\"")) t = t.Substring(0, t.Length - 1);

                t = t.Trim();
                if(string.IsNullOrEmpty(t))
                {
                    continue;
                }

                if(t.StartsWith("#"))
                {
                    t = t.Substring(1, s.Length - 1);
                    model.Tags.Add(t);
                }
                else if(reservedTerms.Contains(t))
                {
                    model.Reserved.Add(t);
                }
                else
                {
                    model.Other.Add(t);
                }
            }

            return model;
        }
    }
}
