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

        [Required(ErrorMessage = "Please enter the name.")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(10, ErrorMessage = "Please use less than 10 characters.")]
        public string Encoding { get; set; }

        [MaxLength(20, ErrorMessage = "Please use less than 20 characters.")]
        public string WindowName { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 100 characters.")]
        [Required(ErrorMessage = "Please enter the URL.")]
        public string Url { get; set; }

        [Display(Name = "Send sentence?")]
        public bool Sentence { get; set; }

        [Display(Name = "Automatically open?")]
        public bool AutoOpen { get; set; }
    }

    public class LanguageModel
    {
        public Guid LanguageId { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [MaxLength(2, ErrorMessage = "Please use less than 2 characters.")]
        public string Code { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 50 characters.")]
        public string RegexSplitSentences { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 100 characters.")]
        public string RegexWordCharacters { get; set; }
        public LanguageDirection Direction { get; set; }
        public bool ShowSpaces { get; set; }

        public Dictionary<string, string> Languages { get; set; }
    }
}