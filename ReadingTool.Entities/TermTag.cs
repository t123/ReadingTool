using System;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class Tag
    {
        public virtual long TagId { get; set; }
        private string _tagTerm;
        public virtual string TagTerm
        {
            get { return _tagTerm; }
            set { _tagTerm = value.Trim().ToLowerInvariant(); }
        }
    }

    public class TagMap : ClassMap<Tag>
    {
        public TagMap()
        {
            Id(x => x.TagId).GeneratedBy.Identity();
            Map(x => x.TagTerm).Length(50).Not.Nullable().Unique();
        }
    }
}