using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ReadingTool.Core.FilterParser
{
    public class FilterParser
    {
        public static readonly string[] MagicTextTags = new[]
            {
                @"parallel",
                @"audio",
                @"noaudio",
                @"single",
                @"recent",
                @"new",
                @"unseen",
            };

        public static readonly string[] MagicTermTags = new[]
            {
                @"box1",
                @"box2",
                @"box3",
                @"box4",
                @"box5",
                @"box6",
                @"box7",
                @"box8",
                @"box9",
                @"known",
                @"unknown",
                @"ignored",
                @"notseen",
                @"definitions",
                @"nodefinitions"
            };

        public class FilterResult
        {
            public List<string> UserLanguages { get; set; }
            public List<string> Languages { get; set; }
            public List<string> Tags { get; set; }
            public List<string> Other { get; set; }
            public List<string> Magic { get; set; }

            public FilterResult()
            {
                UserLanguages = new List<string>();
                Languages = new List<string>();
                Tags = new List<string>();
                Other = new List<string>();
                Magic = new List<string>();
            }
        }

        private static readonly Regex regex = new Regex(@"#[\w|\-]+|(\w|\-)+|""(\w|\s|\-)*""");

        private static string StripQuotes(string input)
        {
            return input.Replace("\"", "");
        }
        public static FilterResult Parse(IEnumerable<string> userLanguages, string filter, string[] magicTerms)
        {
            filter = (filter ?? "").ToLowerInvariant().Trim();
            var result = new FilterResult() { UserLanguages = userLanguages.ToList() };

            foreach(Match s in regex.Matches(filter))
            {
                var t = s.Value;

                if(t.StartsWith("\"")) t = t.Substring(1, t.Length - 1);
                if(t.EndsWith("\"")) t = t.Substring(0, t.Length - 1);

                t = StripQuotes(t.Trim());
                if(string.IsNullOrEmpty(t))
                {
                    continue;
                }

                if(t.StartsWith("#"))
                {
                    t = t.Substring(1, s.Length - 1);
                    if(magicTerms.Contains(t))
                    {
                        result.Magic.Add(t);
                    }
                    else
                    {
                        result.Tags.Add(t);
                    }

                    continue;
                }

                if(userLanguages.Contains(t))
                {
                    result.Languages.Add(t);
                    continue;
                }

                result.Other.Add(t);
            }

            return result;
        }
    }
}
