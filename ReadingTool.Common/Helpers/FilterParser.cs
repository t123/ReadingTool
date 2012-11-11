using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using ReadingTool.Common.Keys;

namespace ReadingTool.Common.Helpers
{
    public class FilterParser
    {
        private static Regex regex = new Regex(@"#[\w]+|\w+|""[\w\s]*""");

        public static FilterModel ParseTerms(string[] userLanguages, string filter)
        {
            filter = (filter ?? "").ToLowerInvariant().Trim();
            FilterModel model = new FilterModel() { UserLanguages = userLanguages };

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
                else if(userLanguages.Contains(t))
                {
                    model.Languages.Add(t);
                }
                else
                {
                    model.Other.Add(t);
                }
            }

            return model;
        }

        public static FilterModel ParseItems(string[] userLanguages, string filter)
        {
            filter = (filter ?? "").ToLowerInvariant().Trim();
            FilterModel model = new FilterModel() { UserLanguages = userLanguages };

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
                else if(userLanguages.Contains(t))
                {
                    model.Languages.Add(t);
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