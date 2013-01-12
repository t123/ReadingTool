using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core.Enums;

namespace ReadingTool.Entities
{
    public class LanguageSettings
    {
        [MaxLength(100)]
        public string CharacterSubstitutions { get; set; }

        [MaxLength(100)]
        public string RegexSplitSentences { get; set; }

        [MaxLength(100)]
        public string ExceptionSplitSentences { get; set; }

        [MaxLength(100)]
        public string RegexWordCharacters { get; set; }

        [Display(Name = "Show Romanisation Field")]
        public bool ShowRomanisation { get; set; }

        public bool RemoveSpaces { get; set; }

        public bool SplitEachCharacter { get; set; }

        public bool KeepFocus { get; set; }
        public LanguageDirection Direction { get; set; }
        public ModalBehaviour ModalBehaviour { get; set; }
    }
}
