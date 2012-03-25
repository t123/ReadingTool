#region License
// SignUpModel.cs is part of ReadingTool.Models
// 
// ReadingTool.Models is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Models is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Models. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.User
{
    public class SignUpModel
    {
        [Required]
        [Remote("UsernameInUse", "RemoteValidator", HttpMethod = "POST")]
        [RegularExpression(@"[A-Za-z](?=[A-Za-z0-9_.]{3,31}$)[a-zA-Z0-9_]*\.?[a-zA-Z0-9_]*$", ErrorMessage = "Use 4 to 32 characters and start with a letter. You may use letters, numbers, underscores, and one dot (.).")]
        [Help("Your username must be unique, between 4 and 32 characters and start with a letter. You may use letters, numbers, underscores, and one dot (.).")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Help("A password is required, but you may choose anything.")]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}