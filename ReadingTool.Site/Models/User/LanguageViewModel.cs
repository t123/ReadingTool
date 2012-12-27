using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ReadingTool.Core.Attributes;
using ReadingTool.Site.Attributes;

namespace ReadingTool.Site.Models.User
{
    public class LanguageViewModel
    {
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public long Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Language Name", Order = 1)]
        public string Name { get; set; }

        [Display(Name = "Colour", Order = 2)]
        [Required]
        [StringLength(7, MinimumLength = 7)]
        [Tip("This colour identifies your language")]
        public string Colour { get; set; }

        [Display(Name = "System Language", Order = 3)]
        public string SystemLanguage { get; set; }

        public LanguageSettingsViewModel Settings { get; set; }
    }
}