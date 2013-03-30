#region License
// TextModel.cs is part of ReadingTool.Site
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Texts
{
    public class TextModel
    {
        public Guid TextId { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        public string Title { get; set; }
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        [Display(Name = "Collection Name")]
        public string CollectionName { get; set; }
        [Display(Name = "Collection Number")]
        public int? CollectionNo { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [Display(Name = "Language 1")]
        public Guid Language1Id { get; set; }

        [Display(Name = "Language 2")]
        public Guid? Language2Id { get; set; }

        [Display(Name = "Text")]
        public string L1Text { get; set; }

        [Display(Name = "Parallel Text")]
        public string L2Text { get; set; }

        public Dictionary<Guid, string> LanguageList { get; set; }

        [Display(Name = "Audio URL")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        public string AudioUrl { get; set; }
    }
}