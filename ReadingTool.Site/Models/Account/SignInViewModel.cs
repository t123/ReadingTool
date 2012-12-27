using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Account
{
    public class SignInViewModel
    {
        [Required]
        [Display(Name = "Username", Order = 1)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }
    }
}