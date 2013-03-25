using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Account
{
    public class AccountModel
    {
        public class UserModel
        {
            public long UserId { get; set; }

            [Display(Name = "Email Address")]
            [MaxLength(50, ErrorMessage = "Please use less than 100 characters.")]
            public string EmailAddress { get; set; }
            public string Username { get; set; }
            [Display(Name = "Display Name")]
            [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
            public string DisplayName { get; set; }
            public DateTime Created { get; set; }
            public string[] Roles { get; set; }
        }

        public class PasswordModel
        {
            [Display(Name = "New Password")]
            [Required(ErrorMessage = "Please enter your new password.")]
            public string NewPassword { get; set; }

            [Display(Name = "Current Password")]
            [Required(ErrorMessage = "Please enter your current password.")]
            public string OldPassword { get; set; }
        }

        public class DeleteModel
        {
            [Display(Name = "Password")]
            [Required(ErrorMessage = "Please enter your password.")]
            public string Password { get; set; }
        }

        public UserModel User { get; set; }
        public PasswordModel Password { get; set; }
        public DeleteModel Delete { get; set; }

        public AccountModel()
        {
            User = new UserModel();
            Password = new PasswordModel();
            Delete = new DeleteModel();
        }
    }

}