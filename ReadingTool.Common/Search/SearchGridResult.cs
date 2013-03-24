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