#region License
// WordSearchModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Search
{
    public class WordSearchItemModel
    {
        public string Id { get; set; }
        public string LanguageName { get; set; }
        public string LanguageColour { get; set; }
        public string Word { get; set; }
        public string State { get; set; }
        public int Box { get; set; }
        public string Definition { get; set; }
        public string Sentence { get; set; }
    }
}