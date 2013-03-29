#region License
// SearchGridPaging.cs is part of ReadingTool.Common
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

using System;

namespace ReadingTool.Common.Search
{
    public class SearchGridPaging
    {
        public const int DefaultRows = 20;

        public int RowsPerPage { get; set; }
        public int TotalRows { get; set; }
        public int Page { get; set; }
        public int TotalPages
        {
            get { return (int)Math.Ceiling(this.TotalRows / (double)this.RowsPerPage); }
        }

        public SearchGridPaging()
        {
            RowsPerPage = DefaultRows;
        }
    }
}
