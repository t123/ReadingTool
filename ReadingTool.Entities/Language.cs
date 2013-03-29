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
// Copyright (C) 2013 Travis Watt
#endregion

using System.Collections.Generic;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Language
    {
        public virtual long LanguageId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual User User { get; set; }
        public virtual IList<UserDictionary> Dictionaries { get; set; }

        private string _jsonSettings;

        public virtual LanguageSettings Settings
        {
            get { return _jsonSettings == null ? new LanguageSettings() : ServiceStack.Text.TypeSerializer.DeserializeFromString<LanguageSettings>(_jsonSettings); }
            set { _jsonSettings = ServiceStack.Text.TypeSerializer.SerializeToString(value); }
        }

        public class LanguageSettings
        {
            public bool ShowSpaces { get; set; }
            public bool Modal { get; set; }
            public string RegexSplitSentences { get; set; }
            public string RegexWordCharacters { get; set; }
            public LanguageDirection Direction { get; set; }
        }

        public Language()
        {
            Dictionaries = new List<UserDictionary>();
        }
    }

    public class LanguageMap : ClassMap<Language>
    {
        public LanguageMap()
        {
            Id(x => x.LanguageId).GeneratedBy.Identity();
            Map(x => x.Name).Length(50).Not.Nullable().Index("IDX_Language_Name");
            Map(x => x.Code).Length(2).Not.Nullable();
            Map(Reveal.Member<Language>("_jsonSettings")).Column("Settings").Length(10000);
            References(x => x.User).Not.Nullable();
            HasMany(x => x.Dictionaries)
                //.Not.LazyLoad()
                .Cascade
                .AllDeleteOrphan()
                .Inverse();
        }
    }
}