#region License
// LanguageModel.cs is part of ReadingTool.Models
// 
// ReadingTool.Models is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Models is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Models. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Create.Language
{
    public class LanguageModel
    {
        public ObjectId LanguageId { get; set; }

        [Required]
        [Help("This your name for the language, for example Deutsch, 日本語, Türkçe etc.")]
        public string Name { get; set; }

        [DisplayName("Language")]
        [Help("This is the name of the language you are studying. This option is here to group users who may have different names for the same language.")]
        [Remote("ValidateSystemLanguageName", "RemoteValidator", HttpMethod = "POST")]
        public string SystemLanguageName { get; set; }

        [DisplayName("Automatically open dictionary")]
        [Help("If you want a dictionary to automatically open when the text popup opens, choose it here.")]
        public string DefaultDictionary { get; set; }

        [DisplayName("Translate Url")]
        [Help("You can supply a translation URL here, such as Google Translate. You can use [[text]] to send the current sentence. The format for Google Translate from German to English for example is: http://translate.google.com/?ie=UTF-8&sl=de&tl=en&text=[[text]]")]
        public string TranslateUrl { get; set; }

        [DisplayName("Right to left language?")]
        [Help("This sets the direction of the text from right to left.")]
        public bool IsRtlLanguage { get; set; }

        [DisplayName("Show a romanisation field?")]
        [Help("If you want to fill in romanised text, check this box. Otherwise it is not displayed.")]
        public bool HasRomanisationField { get; set; }

        [DisplayName("Remove spaces?")]
        [Help("This removes the spaces between words. You can toggle this from the reading/watching section as well.")]
        public bool RemoveSpaces { get; set; }

        [DisplayName("When does the text popup open?")]
        [Help("This determines when the text popup opens. This is useful for software dictionaries that rely on clicking.")]
        public ModalBehaviour ModalBehaviour { get; set; }

        [DisplayName("Punctuation Regular Expression")]
        [Help("<u>Advanced</u>: This is a regular expression to match punctuation. It will be automatically created from the " +
              "punctuation above if blank.")]
        public string PunctuationRegEx { get; set; }

        [Help("These are punctuation tokens separated by a space.")]
        public string Punctuation { get; set; }

        [DisplayName("Sentence End Markers")]
        [Help("TThis is a regular expression to match the end of a sentence.")]
        public string SentenceEndRegEx { get; set; }

        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        [Help("Choose a colour to help identify this language.")]
        public string Colour { get; set; }

        [DisplayName("Keep focus on text?")]
        [Help("When the dictionary opens, you can choose whether focus remains on the text or switches to the dictionary window. If you choose no " +
            "you will have to refocus on the text modal window to enter information (with your mouse/keyboard/touch etc).")]
        public bool KeepFocus { get; set; }

        public LanguageModel()
        {
            KeepFocus = true;
            Punctuation = @"» « , ! : \ / & £ % ~ @ ; # ] } "" ^ $ ( ) < [ { \ | > . * + ? “ ” 。 ― 、… -";
            SentenceEndRegEx = @"[.?!。]|$";
        }
    }
}