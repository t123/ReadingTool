using System;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class SystemLanguage
    {
        public virtual int SystemLanguageId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
    }

    public class SystemLanguageMap : ClassMap<SystemLanguage>
    {
        public SystemLanguageMap()
        {
            Id(x => x.SystemLanguageId).GeneratedBy.Identity();
            Map(x => x.Name).Length(50).Not.Nullable();
            Map(x => x.Code).Length(2).Not.Nullable().UniqueKey("LanguageCode");
        }
    }
}