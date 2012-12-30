using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core.Attributes;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Models.Admin
{
    [Description("Public Language")]
    public class PublicLanguageViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Language Name", Order = 1)]
        public string Name { get; set; }

        public LanguageSettingsViewModel Settings { get; set; }
    }
}