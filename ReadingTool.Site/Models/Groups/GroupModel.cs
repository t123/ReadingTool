#region License
// LanguageViewModel.cs is part of ReadingTool.Site
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
using ReadingTool.Common;

namespace ReadingTool.Site.Models.Groups
{
    public class GroupModel
    {
        public Guid GroupId { get; set; }

        [MaxLength(50, ErrorMessage = "The name must be less than 50 characters.")]
        [MinLength(3, ErrorMessage = "The name must be more than 3 letters.")]
        [RegularExpression(@"([\d\w\s]+)", ErrorMessage = "Only letters, numbers and spaces are allowed.")]
        public string Name { get; set; }

        [MaxLength(1000, ErrorMessage = "The description must be less than 1000 characters.")]
        public string Description { get; set; }

        [Display(Name = "Group Type")]
        public GroupType GroupType { get; set; }

        public GroupMembershipModel CurrentUser { get; set; }
    }
}