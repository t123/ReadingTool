using System.Collections.Generic;

namespace ReadingTool.Common.Search
{
    public class SearchResult<T>
    {
        public int TotalRows { get; set; }
        public IEnumerable<T> Results { get; set; }
    }
}