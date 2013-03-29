#region License
// SearchOptions.cs is part of ReadingTool.Common
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
namespace ReadingTool.Common.Search
{
    public class SearchOptions
    {
        private string _sort;
        private int _rowsPerPage;

        public int RowsPerPage
        {
            get { return _rowsPerPage; }
            set
            {
                if(value <= 0)
                {
                    _rowsPerPage = 20;
                }
                else if(value > 250)
                {
                    _rowsPerPage = 250;
                }
                else
                {
                    _rowsPerPage = value;
                }
            }
        }

        private int _page;
        public int Page
        {
            get { return _page; }
            set { _page = value < 1 ? 1 : value; }
        }


        public string Sort { get { return _sort; } set { _sort = (value ?? "").ToLowerInvariant(); } }
        public GridSortDirection Direction { get; set; }
        public string Filter { get; set; }
        public bool IgnorePaging { get; set; }
        public int Skip { get { return (Page - 1) * RowsPerPage; } }

        public SearchOptions()
        {
            Page = 1;
            Direction = GridSortDirection.Asc;
        }
    }
}