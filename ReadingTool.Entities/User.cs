using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class User
    {
        public virtual Guid UserId { get; set; }
        public virtual string Username { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual string Password { get; set; }

        private string _roles;
        public virtual string[] Roles { get { return _roles.Split(','); } set { _roles = string.Join(",", value ?? new string[] { }); } }

        public virtual IList<Text> Texts { get; set; }
        public virtual IList<Term> Terms { get; set; }
        public virtual IList<Language> Languages { get; set; }

        //public virtual string CurrentName
        //{
        //    get { return string.IsNullOrWhiteSpace(DisplayName) ? Username : DisplayName; }
        //}

        public User()
        {
            Texts = new List<Text>();
            Terms = new List<Term>();
            Languages = new List<Language>();
        }
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.UserId).GeneratedBy.GuidComb();
            Map(x => x.Username).Length(50).Not.Nullable().UniqueKey("username").Index("IDX_User_Username");
            Map(x => x.DisplayName).Length(50);
            Map(x => x.EmailAddress).Length(100);
            Map(x => x.Created);
            Map(x => x.Password).Length(100).Not.Nullable();
            Map(Reveal.Member<User>("_roles")).Column("Roles").Length(25);
            HasMany(x => x.Terms).Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Texts).Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Languages).Cascade.AllDeleteOrphan().Inverse();
        }
    }
}
