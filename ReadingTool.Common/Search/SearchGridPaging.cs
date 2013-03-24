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
