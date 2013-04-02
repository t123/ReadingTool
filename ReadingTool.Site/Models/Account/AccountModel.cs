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

using System;
using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Account
{
    public class UserModel
    {
        public Guid UserId { get; set; }

        [Display(Name = "Email Address")]
        [MaxLength(50, ErrorMessage = "Please use less than 100 characters.")]
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        [Display(Name = "Display Name")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        public string DisplayName { get; set; }
        public DateTime Created { get; set; }
        //public string[] Roles { get; set; }

        [Display(Name = "Your API Key")]
        public string ApiKey { get; set; }
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
}