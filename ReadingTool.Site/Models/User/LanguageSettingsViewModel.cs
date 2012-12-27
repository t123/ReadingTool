using System.ComponentModel.DataAnnotations;
using ReadingTool.Core.Enums;

namespace ReadingTool.Site.Models.User
{
    public class LanguageSettingsViewModel
    {
        [Display(Name = "Character Substitutions", Order = 1)]
        public string CharacterSubstitutions { get; set; }

        [Display(Name = "Regex Split Sentences", Order = 2)]
        public string RegexSplitSentences { get; set; }

        [Display(Name = "Exception Split Sentences", Order = 3)]
        public string ExceptionSplitSentences { get; set; }

        [Display(Name = "Regex Word Characters", Order = 4)]
        public string RegexWordCharacters { get; set; }

        [Display(Name = "Show Romanisation", Order = 5)]
        public bool ShowRomanisation { get; set; }

        [Display(Name = "Remove Spaces", Order = 6)]
        public bool RemoveSpaces { get; set; }

        [Display(Name = "Split Each Character", Order = 7)]
        public bool SplitEachCharacter { get; set; }

        [Display(Name = "Keep Focus", Order = 8)]
        public bool KeepFocus { get; set; }

        [Display(Name = "Language Direction", Order = 9)]
        public LanguageDirection Direction { get; set; }

        [Display(Name = "Modal Behaviour", Order = 10)]
        public ModalBehaviour ModalBehaviour { get; set; }

        public static LanguageSettingsViewModel Default
        {
            get
            {
                return new LanguageSettingsViewModel()
                    {
                        CharacterSubstitutions = @"´='|`='|’='|‘='|...=…|..=‥|»=|«=|“=|”=|„=|‟=|""=",
                        RegexWordCharacters = @"a-zA-ZÀ-ÖØ-öø-ȳ",
                        RegexSplitSentences = ".!?:;",
                        ExceptionSplitSentences = "[A-Z].|Dr.",
                        KeepFocus = true,
                        Direction = LanguageDirection.LTR,
                        ModalBehaviour = ModalBehaviour.LeftClick
                    };
            }
        }
    }
}