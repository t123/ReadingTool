#region License
// SearchGridResult.cs is part of ReadingTool.Common
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
// Copyright (C) 2013 Travis Watt
#endregion

using System.Collections.Generic;

namespace ReadingTool.Common.Search
{
    public class SearchGridResult<T>
    {
        public string Sort { get; set; }
        public GridSortDirection Direction { get; set; }
        public IEnumerable<T> Items { get; set; }
        public SearchGridPaging Paging { get; set; }

        public SearchGridResult()
        {
            Paging = new SearchGridPaging();
        }
    }
}