#region License
// TagHelper.cs is part of ReadingTool.Common
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
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadingTool.Common.Helpers
{
    public class TagHelper
    {
        public static class Tags
        {
            public const string AUDIO = @"audio";
            public const string NO_AUDIO = @"noaudio";
            public const string NEW = @"new";
            public const string ARCHIVE = @"archive";
            public const string PARALLEL = @"parallel";
            public const string SHARED = @"shared";
        }

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
    }
}