#region License
// UserDictionary.cs is part of ReadingTool.Entities
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
    public class UserDictionary
    {
        public virtual long DictionaryId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Encoding { get; set; }
        public virtual string WindowName { get; set; }
        public virtual string Url { get; set; }
        public virtual bool Sentence { get; set; }
        public virtual Language Language { get; set; }
        public virtual bool AutoOpen { get; set; }
    }

    public class UserDictionaryMap : ClassMap<UserDictionary>
    {
        public UserDictionaryMap()
        {
            Id(x => x.DictionaryId).GeneratedBy.Identity();
            Map(x => x.Name).Length(50);
            Map(x => x.Encoding).Length(10);
            Map(x => x.WindowName).Length(20);
            Map(x => x.Url).Length(100);
            Map(x => x.Sentence);
            Map(x => x.AutoOpen);
            References(x => x.Language);
        }
    }
}