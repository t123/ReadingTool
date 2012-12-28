using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ReadingTool.Core.Attributes;
using ReadingTool.Core.Enums;

namespace ReadingTool.Site.Models.User
{
    [Description("Dictionary")]
    public class DictionaryViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [ReadOnly(true)]
        [HiddenInput(DisplayValue = false)]
        public Guid LanguageId { get; set; }

        [Display(Name = "Dictionary Name", Order = 1)]
        [Tip("This is your name for the dictionary.")]
        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [Display(Name = "Dictionary URL", Order = 2)]
        [Required]
        //[Url]
        [StringLength(250)]
        [DataType(DataType.Url)]
        public string Url { get; set; }

        [Display(Name = "Window Name", Order = 3)]
        [Tip("You can group dictionaries by forcing them to open in the same window or tab with this name.")]
        [StringLength(30)]
        public string WindowName { get; set; }

        [Display(Name = "Url Encoding", Order = 4)]
        [StringLength(10)]
        public string UrlEncoding { get; set; }

        [Display(Name = "Dictionary or Translation?", Order = 5)]
        [Required]
        public DictionaryParameter Parameter { get; set; }

        [Display(Name = "Open immediately", Order = 6)]
        [Tip("This will open the dictionary as soon as a word is chosen.")]
        [Required]
        public bool AutoOpen { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public short DisplayOrder { get; set; }
    }
}