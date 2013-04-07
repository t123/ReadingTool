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
using ReadingTool.Site.Attributes;

namespace ReadingTool.Site.Models.Texts
{
    public class TextModel
    {
        public Guid TextId { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        [Tip("The title of your text.")]
        public string Title { get; set; }

        [MaxLength(50, ErrorMessage = "Please use less than 50 characters.")]
        [Display(Name = "Collection Name")]
        [Tip("The collection, if any, this text belongs to.")]
        public string CollectionName { get; set; }

        [Display(Name = "Collection Number")]
        [Tip("The collection number is used for ordering texts within a collection.")]
        public int? CollectionNo { get; set; }

        [Required(ErrorMessage = "Please select a language.")]
        [Tip("The language this text is in.")]
        [Display(Name = "Language 1")]
        public Guid Language1Id { get; set; }

        [Display(Name = "Language 2")]
        [Tip("If the text is a parallel text, the language of the parallel text.")]
        public Guid? Language2Id { get; set; }

        [Display(Name = "Text")]
        public string L1Text { get; set; }

        [Display(Name = "Parallel Text")]
        public string L2Text { get; set; }

        public Dictionary<Guid, string> LanguageList { get; set; }

        [Display(Name = "Audio URL")]
        [MaxLength(250, ErrorMessage = "Please use less than 250 characters.")]
        [Tip("The URL of the audio for this text.")]
        public string AudioUrl { get; set; }

        [Display(Name = "Share the audio?")]
        [Tip("Check this box to share the audio URL if you choose to share this text.")]
        public bool ShareAudioUrl { get; set; }
    }
}