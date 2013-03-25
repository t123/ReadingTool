using System;

namespace ReadingTool.Site.Models.Languages
{
    public class DictionaryViewModel
    {
        public long DictionaryId { get; set; }
        public long LanguageId { get; set; }
        public string Name { get; set; }
        public string Encoding { get; set; }
        public string WindowName { get; set; }
        public string Url { get; set; }
        public bool Sentence { get; set; }
        public bool AutoOpen { get; set; }
    }
}