using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReadingTool.Common;

namespace ReadingTool.Site.Models.Languages
{
    public class LanguageEditModel
    {
        public LanguageModel Language { get; set; }
        public IList<DictionaryViewModel> Dictionaries { get; set; }

        public LanguageEditModel()
        {
            Dictionaries = new List<DictionaryViewModel>();
        }
    }

    public class DictionaryModel
    {
        public Guid DictionaryId { get; set; }
        public Guid LanguageId { get; set; }
        public string Name { get; set; }
        public string Encoding { get; set; }
        public string WindowName { get; set; }
        public string Url { get; set; }
        public bool Sentence { get; set; }
        public bool AutoOpen { get; set; }
    }

    public class LanguageModel
    {
        public Guid LanguageId { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        public string Code { get; set; }

        public string RegexSplitSentences { get; set; }
        public string RegexWordCharacters { get; set; }
        public LanguageDirection Direction { get; set; }
        public bool ShowSpaces { get; set; }

        public Dictionary<string, string> Languages { get; set; }
    }
}