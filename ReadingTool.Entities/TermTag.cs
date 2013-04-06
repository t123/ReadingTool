#region License
// TermTag.cs is part of ReadingTool.Entities
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
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class Tag
    {
        public virtual Guid TagId { get; set; }
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
            Cache.ReadWrite();
            Id(x => x.TagId).GeneratedBy.GuidComb();
            Map(x => x.TagTerm).Length(50).Not.Nullable().Unique().Index("IDX_Tag_TagTerm");
        }
    }
}