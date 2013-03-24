using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Texts
{
    public class TextModel
    {
        public Guid TextId { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        public string Title { get; set; }
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        [Display(Name = "Collection Name")]
        public string CollectionName { get; set; }
        [Display(Name = "Collection Number")]
        public int? CollectionNo { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [Display(Name = "Language 1")]
        public Guid Language1Id { get; set; }

        [Display(Name = "Language 2")]
        public Guid? Language2Id { get; set; }

        [Display(Name = "Text")]
        public string L1Text { get; set; }

        [Display(Name = "Parallel Text")]
        public string L2Text { get; set; }

        public Dictionary<Guid, string> LanguageList { get; set; }

        [Display(Name = "Audio URL")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        public string AudioUrl { get; set; }
    }
}