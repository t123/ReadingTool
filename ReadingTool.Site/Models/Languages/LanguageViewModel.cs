using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Languages
{
    public class LanguageViewModel
    {
        public Guid LanguageId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public IList<DictionaryViewModel> Dictionaries { get; set; }

        public LanguageViewModel()
        {
            Dictionaries = new List<DictionaryViewModel>();
        }
    }
}