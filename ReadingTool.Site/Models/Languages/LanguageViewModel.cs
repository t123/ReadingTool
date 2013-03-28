using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Languages
{
    public class LanguageViewModel
    {
        public long LanguageId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public IList<DictionaryViewModel> Dictionaries { get; set; }
        public bool Modal { get; set; }
        public bool ShowSpaces { get; set; }

        public LanguageViewModel()
        {
            Dictionaries = new List<DictionaryViewModel>();
        }
    }
}