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

        private static Regex regex = new Regex(@"#[\w]+|\w+|""[\w\s]*""");

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
