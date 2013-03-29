#region License
// User.cs is part of ReadingTool.Entities
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

using System;
using System.Collections.Generic;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class User
    {
        public virtual long UserId { get; set; }
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
            Id(x => x.UserId).GeneratedBy.Identity();
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
