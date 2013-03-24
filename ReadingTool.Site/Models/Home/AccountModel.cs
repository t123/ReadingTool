using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Home
{
    public class AccountModel
    {
        public class RegisterModel
        {
            [Required(ErrorMessage = "Please enter a username.")]
            [MinLength(3, ErrorMessage = "The username must be more than 3 letters.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Please enter a password.")]
            public string Password { get; set; }
        }

        public class SignInModel
        {
            [Required(ErrorMessage = "Please enter your username.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Please enter your password.")]
            public string Password { get; set; }
        }

        public RegisterModel Register { get; set; }
        public SignInModel SignIn { get; set; }

        public AccountModel()
        {
            Register = new RegisterModel();
            SignIn = new SignInModel();
        }
    }
}