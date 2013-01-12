using System.Collections.Generic;

namespace ReadingTool.Entities.Search
{
    public class SearchResult<T>
    {
        public int TotalRows { get; set; }
        public IEnumerable<T> Results { get; set; }
    }
}