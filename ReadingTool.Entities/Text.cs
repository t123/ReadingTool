using System;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Text
    {
        public virtual Guid TextId { get; set; }
        public virtual string Title { get; set; }
        public virtual string CollectionName { get; set; }
        public virtual int? CollectionNo { get; set; }
        public virtual Language Language1 { get; set; }
        public virtual Language Language2 { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual DateTime? LastRead { get; set; }
        public virtual User User { get; set; }
        public virtual string L1Text { get; set; }
        public virtual string L2Text { get; set; }
        public virtual string AudioUrl { get; set; }
    }

    public class TextMap : ClassMap<Text>
    {
        public TextMap()
        {
            Id(x => x.TextId).GeneratedBy.GuidComb();
            Map(x => x.Title).Length(250).Not.Nullable();
            Map(x => x.CollectionName).Length(250);
            Map(x => x.CollectionNo);
            References(x => x.Language1);
            References(x => x.Language2);
            References(x => x.User).Not.Nullable();
            Map(x => x.Created).Not.Nullable();
            Map(x => x.Modified).Not.Nullable();
            Map(x => x.LastRead);
            Map(x => x.AudioUrl).Length(250);
            Map(x => x.L1Text).Length(10000);
            Map(x => x.L2Text).Length(10000);
        }
    }
}