using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ServiceStack.OrmLite;

namespace ReadingTool.Site.Models.WebApi
{
    public class LanguageModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Colour { get; set; }
        public LanguageSettingsModel Settings { get; set; }
        public string SystemLangcodeCode { get; set; }
        public string SystemLangcodeName { get; set; }
        public IEnumerable<LanguageDictionaryModel> Dictionaries { get; set; }
    }
}
