using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ReadingTool.Core.Attributes;
using ReadingTool.Site.Attributes;
using ServiceStack.DataAnnotations;

namespace ReadingTool.Site.Models.User
{
    public class ProfileViewModel
    {
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Username", Order = 1)]
        [Tip("This is your unique username.")]
        [MinLength(3)]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "Please use only letters, numbers, hyphens and underscores")]
        public string Username { get; set; }

        [StringLength(30)]
        [Display(Name = "Display Name", Order = 2)]
        [Tip("This is the name other users see, it can be anything you like.")]
        public string DisplayName { get; set; }

        [ValidEmailAddress]
        [StringLength(50)]
        [Display(Name = "Email Address", Order = 3)]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public DateTime Created { get; set; }

        [StringLength(10)]
        [Display(Name = "Theme", Order = 4)]
        [Tip("Themes are provided by http://bootswatch.com/")]
        public string Theme { get; set; }
    }
}