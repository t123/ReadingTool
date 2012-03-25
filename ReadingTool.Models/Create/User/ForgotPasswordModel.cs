﻿#region License
// ForgotPasswordModel.cs is part of ReadingTool.Models
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

using System.ComponentModel.DataAnnotations;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.User
{
    public class ForgotPasswordModel
    {
        [Help("If you filled in your email address, fill in your username to send password reset instructions.")]
        [Required]
        public string Username { get; set; }
    }
}