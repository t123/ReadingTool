using ReadingTool.Core.Enums;

namespace ReadingTool.Entities.Search
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

        public SearchOptions()
        {
            Page = 1;
            Direction = GridSortDirection.Asc;
        }
    }
}