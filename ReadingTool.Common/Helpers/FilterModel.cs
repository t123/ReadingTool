using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadingTool.Common.Helpers
{
    public class FilterModel
    {
        public IList<string> UserLanguages { get; set; }
        public IList<string> Languages { get; set; }
        public IList<string> Tags { get; set; }
        public IList<string> Other { get; set; }

        public FilterModel()
        {
            UserLanguages = new List<string>();
            Languages = new List<string>();
            Tags = new List<string>();
            Other = new List<string>();
        }
    }
}
