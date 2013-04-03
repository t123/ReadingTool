using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Group
    {
        public virtual Guid GroupId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual GroupType GroupType { get; set; }
        public virtual ICollection<Text> Texts { get; set; }
        public virtual IList<GroupMembership> Members { get; set; }

        public Group()
        {
            Texts = new List<Text>();
            Members = new List<GroupMembership>();
        }
    }

    public class GroupMap : ClassMap<Group>
    {
        public GroupMap()
        {
            Id(x => x.GroupId).GeneratedBy.GuidComb();
            Map(x => x.Name).Length(50).Not.Nullable().UniqueKey("groupname").Index("IDX_Group_Name");
            Map(x => x.Description).Length(1000);
            Map(x => x.GroupType).CustomType<GroupType>();
            HasMany(x => x.Members).Cascade.AllDeleteOrphan().Inverse();

            HasManyToMany<Text>(x => x.Texts)
                .Table("GroupText")
                .ParentKeyColumn("GroupId")
                .ChildKeyColumn("TextId")
                .Cascade
                .All()
                .BatchSize(100)
                .AsSet()
                ;
        }
    }
}
