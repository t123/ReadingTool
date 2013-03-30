#region License
// LanguageModel.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

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
        [Display(Name = "Regex Split Sentences")]
        public string RegexSplitSentences { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 100 characters.")]
        [Display(Name = "Regex Test Words")]
        public string RegexWordCharacters { get; set; }
        public LanguageDirection Direction { get; set; }

        [Display(Name = "Display Spaces?")]
        public bool ShowSpaces { get; set; }

        [Display(Name = "Open dictionaries in modal instead of tab?")]
        public bool Modal { get; set; }

        [Display(Name = "When does the word modal open?")]
        public ModalBehaviour ModalBehaviour { get; set; }

        [Display(Name = "Pause audio when word moodal opens?")]
        public bool AutoPause { get; set; }

        public Dictionary<string, string> Languages { get; set; }
    }
}