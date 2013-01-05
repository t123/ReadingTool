using ReadingTool.Core.Enums;

namespace ReadingTool.Site.Models.WebApi
{
    public class LanguageSettingsModel
    {
        public string CharacterSubstitutions { get; set; }
        public string RegexSplitSentences { get; set; }
        public string ExceptionSplitSentences { get; set; }
        public string RegexWordCharacters { get; set; }
        public bool ShowRomanisation { get; set; }
        public bool RemoveSpaces { get; set; }
        public bool SplitEachCharacter { get; set; }
        public bool KeepFocus { get; set; }
        public LanguageDirection Direction { get; set; }
        public ModalBehaviour ModalBehaviour { get; set; }
    }
}
