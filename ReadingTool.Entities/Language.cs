#region License
// Language.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class Language
    {
        public const string CollectionName = @"Languages";
        [BsonId]
        public ObjectId LanguageId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ObjectId SystemLanguageId { get; set; }
        public ObjectId Owner { get; set; }
        public string TranslateUrl { get; set; }
        public bool IsRtlLanguage { get; set; }
        public bool HasRomanisationField { get; set; }
        public bool RemoveSpaces { get; set; }
        public ModalBehaviour ModalBehaviour { get; set; }
        public IList<UserDictionary> Dictionaries { get; set; }
        public string DefaultDictionary { get; set; }
        public string PunctuationRegEx { get; set; }
        public string[] Punctuation { get; set; }
        public string SentenceEndRegEx { get; set; }
        public string Colour { get; set; }
        public bool KeepFocus { get; set; }

        public Language()
        {
            Dictionaries = new List<UserDictionary>();
        }

        public bool ShouldSerializeDictionaries()
        {
            return Dictionaries != null && Dictionaries.Count > 0;
        }

        public static Language Unknown()
        {
            return new Language()
                       {
                           LanguageId = ObjectId.Empty,
                           Name = "Unknown",
                           Colour = ""
                       };
        }

        [BsonIgnore]
        public string ParsingPunctuationExpression
        {
            get
            {
                if(!string.IsNullOrEmpty(PunctuationRegEx)) return PunctuationRegEx;

                StringBuilder sb = new StringBuilder();

                bool hasHyphen = false;

                foreach(var punctuation in Punctuation.Where(x => x.Length > 1).OrderByDescending(x => x.Length))
                {
                    sb.Append("[");
                    sb.Append(Regex.Escape(punctuation));
                    sb.Append("]|");
                }

                sb.Append("[");

                foreach(var punctuation in Punctuation.Where(x => x.Length == 1))
                {
                    if(punctuation == "-")
                    {
                        hasHyphen = true;
                        continue;
                    }

                    sb.Append(Regex.Escape(punctuation).Replace("]", @"\]"));
                }

                sb.Append(" ");
                sb.Append(@"\t");
                sb.Append(@"\r");
                if(hasHyphen)
                {
                    sb.Append("-");
                }

                sb.Append("]");

                return sb.ToString();
            }
        }
    }
}