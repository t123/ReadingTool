#region License
// PublicProfileAvailability.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.ComponentModel;

namespace ReadingTool.Common.Enums
{
    public enum PublicProfileAvailability
    {
        [Description("Nobody, not available at all.")]
        Nobody = 0,

        [Description("Other users of the site.")]
        Users = 1,

        [Description("Everyone, the entire internet.")]
        Everyone = 2
    }
}
