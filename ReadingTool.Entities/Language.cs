using System;
using System.Collections.Generic;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Language
    {
        public virtual Guid LanguageId { get; set; }
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
            Id(x => x.LanguageId).GeneratedBy.GuidComb();
            Map(x => x.Name).Length(50).Not.Nullable().Index("IDX_Language_Name");
            Map(x => x.Code).Length(2).Not.Nullable();
            Map(Reveal.Member<Language>("_jsonSettings")).Column("Settings").Length(10000);
            References(x => x.User).Not.Nullable();
            HasMany(x => x.Dictionaries).Not.LazyLoad().Cascade.AllDeleteOrphan().Inverse();
        }
    }
}