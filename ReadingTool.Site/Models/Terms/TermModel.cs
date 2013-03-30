#region License
// TermModel.cs is part of ReadingTool.Site
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

namespace ReadingTool.Site.Models.Terms
{
    public class TermModel
    {
        public Guid TermId { get; set; }
        public TermState State { get; set; }

        [MaxLength(50, ErrorMessage = "Please use less than 50 characters")]
        public string Phrase { get; set; }

        [Display(Name = "Base Phrase")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters")]
        public string BasePhrase { get; set; }

        [MaxLength(500, ErrorMessage = "Please use less than 500 characters")]
        public string Sentence { get; set; }

        [MaxLength(500, ErrorMessage = "Please use less than 500 characters")]
        public string Definition { get; set; }
        public string Tags { get; set; }
        public short Length { get; set; }
        public short Box { get; set; }
    }
}