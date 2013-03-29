#region License
// TermResponseModel.cs is part of ReadingTool.Api
// 
// ReadingTool.Api is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Api is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Api. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;

namespace ReadingTool.Api.Models.Terms
{
    public class TermResponseModel
    {
        public long TermId { get; set; }
        public string State { get; set; }
        public string Phrase { get; set; }
        public string PhraseLower { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public short Box { get; set; }
        public DateTime? NextReview { get; set; }
        public long TextId { get; set; }
        public long LanguageId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ICollection<string> Tags { get; set; }
        public short Length { get; set; }
    }
}