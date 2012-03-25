#region License
// WordModel.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

namespace ReadingTool.API.Areas.V1.Models
{
    public class WordModel
    {
        public string WordId { get; set; }
        public string LanguageId { get; set; }
        public string WordPhrase { get; set; }
        public string State { get; set; }
        public string Definition { get; set; }
        public string BaseWord { get; set; }
        public string Romanisation { get; set; }
        public string[] Tags { get; set; }
        public string Sentence { get; set; }
        public int Box { get; set; }
        public string Created { get; set; }
        public string Modified { get; set; }
    }
}