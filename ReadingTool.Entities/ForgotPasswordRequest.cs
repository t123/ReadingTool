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
using FluentNHibernate.Mapping;

namespace ReadingTool.Entities
{
    public class ForgotPasswordRequest
    {
        public virtual Guid RequestId { get; set; }
        public virtual User User { get; set; }
        public virtual string ResetKey { get; set; }
        public virtual DateTime Expires { get; set; }
    }

    public class ForgotPasswordRequestMap : ClassMap<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestMap()
        {
            Id(x => x.RequestId).GeneratedBy.GuidComb();
            References(x => x.User).Not.Nullable();
            Map(x => x.ResetKey).Length(50).Not.Nullable();
            Map(x => x.Expires).Not.Nullable();
        }
    }
}