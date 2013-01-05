using System;
using ReadingTool.Core.Enums;

namespace ReadingTool.Site.Models.WebApi
{
    public class LanguageDictionaryModel
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string WindowName { get; set; }
        public string UrlEncoding { get; set; }
        public DictionaryParameter Parameter { get; set; }
        public bool AutoOpen { get; set; }
        public long DisplayOrder { get; set; }
    }
}
