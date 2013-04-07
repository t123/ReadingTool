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
using ReadingTool.Site.Attributes;

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
        [Tip("Your name for the dictionary.")]
        public string Name { get; set; }

        [MaxLength(10, ErrorMessage = "Please use less than 10 characters.")]
        [Tip("The word is encoded in this format before being passed into the dictionary URL.")]
        public string Encoding { get; set; }

        [MaxLength(20, ErrorMessage = "Please use less than 20 characters.")]
        [Display(Name = "Window Name")]
        [Tip("Only used if opening dictionaries in a new tab. Dictionaries will open in the window/tab with this name. Use this option to group dictionaries.")]
        public string WindowName { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 100 characters.")]
        [Required(ErrorMessage = "Please enter the URL.")]
        [Tip("The URL for the dictionary, use ### for the word placement.")]
        public string Url { get; set; }

        [Display(Name = "Send sentence?")]
        [Tip("Check this box if you want to send the sentence instead of the word, for example Google Translate.")]
        public bool Sentence { get; set; }

        [Display(Name = "Automatically open?")]
        [Tip("Check this box if you want to automatically open this dictionary when you click on a word.")]
        public bool AutoOpen { get; set; }
    }

    public class LanguageModel
    {
        public Guid LanguageId { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        [Tip("Your name for the language.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [MaxLength(2, ErrorMessage = "Please use less than 2 characters.")]
        [Tip("This asscociates your language with a system language so texts can be shared between users.")]
        public string Code { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 50 characters.")]
        [Display(Name = "Regex Split Sentences")]
        [Tip("This is a regular expression that is used to split texts into sentences.")]
        public string RegexSplitSentences { get; set; }

        [MaxLength(100, ErrorMessage = "Please use less than 100 characters.")]
        [Display(Name = "Regex Test Words")]
        [Tip("This is a regular expression that is used to test whether a phrase is an actual word or not.")]
        public string RegexWordCharacters { get; set; }

        [Tip("This is the direction of your language.")]
        public LanguageDirection Direction { get; set; }

        [Display(Name = "Display Spaces?")]
        [Tip("If checked spaces are displayed between words, if unchecked sentences are displayed without spaces.")]
        public bool ShowSpaces { get; set; }

        [Display(Name = "Open dictionaries in modal instead of tab?")]
        [Tip("If checked dictionaries are opened in a popup on top of the word popup. If unchecked dictionaries are opened in a new browser tab.")]
        public bool Modal { get; set; }

        [Display(Name = "When does the word modal open?")]
        [Tip("This determines when the word popup is displayed.")]
        public ModalBehaviour ModalBehaviour { get; set; }

        [Display(Name = "Pause audio when word moodal opens?")]
        [Tip("If checked will automatically pause the audio when the word popup is opened and resume when it's closed.")]
        public bool AutoPause { get; set; }

        public Dictionary<string, string> Languages { get; set; }
    }
}