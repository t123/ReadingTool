using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadingTool.Core
{
    public class TagHelper
    {
        public const string TAG_SEPARATOR = @" ";

        public static string[] Split(string tags)
        {
            if(string.IsNullOrEmpty(tags)) return new string[] { };

            return tags
                .ToLowerInvariant()
                .Split(new[] { TAG_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().Replace(" ", "-"))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();
        }

        public static string[] Merge(params string[] tags)
        {
            if(tags == null || tags.Length == 0) return new string[] { };
            List<string> merged = new List<string>();

            foreach(var tag in tags)
            {
                merged.AddRange(Split(tag));
            }

            return merged.Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
        }

        public static string[] Merge(params string[][] tags)
        {
            if(tags == null || tags.Length == 0) return new string[] { };
            List<string> merged = new List<string>();

            foreach(var tag in tags)
            {
                merged.AddRange(tag);
            }

            return merged.Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
        }

        public static string[] Remove(string[] original, params string[][] tags)
        {
            if(tags == null || tags.Length == 0) return new string[] { };

            var updated = new List<string>(original);

            foreach(var tag in tags)
            {
                updated.RemoveAll(x => tag.Contains(x));
            }

            return updated.Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
        }

        public static string ToString(IEnumerable<string> tags)
        {
            return ToString(TAG_SEPARATOR, tags);
        }

        public static string ToString(string join, IEnumerable<string> tags)
        {
            return string.Join(join, tags);
        }
    }
}