#region License
// AccountModel.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System.ComponentModel.DataAnnotations;

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