using System;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class UserDictionary
    {
        public virtual Guid DictionaryId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Encoding { get; set; }
        public virtual string WindowName { get; set; }
        public virtual string Url { get; set; }
        public virtual bool Sentence { get; set; }
        public virtual Language Language { get; set; }
        public virtual bool AutoOpen { get; set; }
    }

    public class UserDictionaryMap : ClassMap<UserDictionary>
    {
        public UserDictionaryMap()
        {
            Id(x => x.DictionaryId).GeneratedBy.GuidComb();
            Map(x => x.Name).Length(50);
            Map(x => x.Encoding).Length(10);
            Map(x => x.WindowName).Length(20);
            Map(x => x.Url).Length(100);
            Map(x => x.Sentence);
            Map(x => x.AutoOpen);
            References(x => x.Language);
        }
    }
}