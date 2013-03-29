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

using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Ajax
{
    public class TermModel
    {
        public long TermId { get; set; }
        public string State { get; set; }
        public string Phrase { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Tags { get; set; }
        public string Message { get; set; }
        public string StateClass { get; set; }
        public short Length { get; set; }

        public bool Exists
        {
            get { return TermId != 0; }
        }

        public short Box { get; set; }
    }

    public class SaveTermModel
    {
        public long? TermId { get; set; }
        public long LanguageId { get; set; }
        public long TextId { get; set; }
        public string State { get; set; }
        [MaxLength(50)]
        public string Phrase { get; set; }
        [MaxLength(50)]
        public string BasePhrase { get; set; }

        [MaxLength(500)]
        public string Sentence { get; set; }

        [MaxLength(500)]
        public string Definition { get; set; }
        public string Tags { get; set; }
    }
}