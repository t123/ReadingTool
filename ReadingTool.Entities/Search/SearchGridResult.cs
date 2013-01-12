using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core.Enums;

namespace ReadingTool.Entities.Search
{
    public class SearchGridResult<T>
    {
        public int RowsPerPage { get; set; }
        public int TotalRows { get; set; }
        public int Page { get; set; }
        public string Sort { get; set; }
        public GridSortDirection Direction { get; set; }
        public IEnumerable<T> Items { get; set; }
        public int TotalPages
        {
            get { return (int)Math.Ceiling(this.TotalRows / (double)this.RowsPerPage); }
        }
    }
}
