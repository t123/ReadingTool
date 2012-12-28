using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class DeleteAccountModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Your Current Password")]
        [Required]
        public string Password { get; set; }
    }
}