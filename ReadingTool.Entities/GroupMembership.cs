using System;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class GroupMembership
    {
        public virtual Guid GroupMembershipId { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        public virtual MembershipType MembershipType { get; set; }
    }

    public class GroupMembershipMap : ClassMap<GroupMembership>
    {
        public GroupMembershipMap()
        {
            Id(x => x.GroupMembershipId).GeneratedBy.GuidComb();
            References(x => x.User);
            References(x => x.Group);
            Map(x => x.MembershipType).CustomType<MembershipType>();
        }
    }
}