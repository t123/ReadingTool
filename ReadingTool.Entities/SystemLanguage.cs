#region License
// SystemLanguage.cs is part of ReadingTool.Entities
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