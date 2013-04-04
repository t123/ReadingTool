#region License
// Text.cs is part of ReadingTool.Entities
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
using System.IO;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using Ionic.Zlib;

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
        public virtual ICollection<Group> Groups { get; set; }

        public Text()
        {
            Groups = new List<Group>();
        }
    }

    public class TextMap : ClassMap<Text>
    {
        public TextMap()
        {
            Id(x => x.TextId).GeneratedBy.Assigned();
            Map(x => x.Title).Length(250).Not.Nullable().Index("IDX_Text_Title");
            Map(x => x.CollectionName).Length(50).Index("IDX_Text_CollectionName");
            Map(x => x.CollectionNo);
            References(x => x.Language1).Index("IDX_Text_Language");
            References(x => x.Language2);
            References(x => x.User).Not.Nullable();
            Map(x => x.Created).Not.Nullable();
            Map(x => x.Modified).Not.Nullable();
            Map(x => x.LastRead);
            Map(x => x.AudioUrl).Length(250);

            HasManyToMany<Text>(x => x.Groups)
                .Table("GroupText")
                .ParentKeyColumn("TextId")
                .ChildKeyColumn("GroupId")
                .Cascade
                .All()
                .BatchSize(100)
                .AsSet()
                ;
        }
    }
}