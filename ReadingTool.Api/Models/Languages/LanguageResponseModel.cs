#region License
// LanguageResponseModel.cs is part of ReadingTool.Api
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

namespace ReadingTool.Api.Models.Languages
{
    public class LanguageResponseModel
    {
        public virtual long LanguageId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public bool ShowSpaces { get; set; }
        public bool Modal { get; set; }
        public string RegexSplitSentences { get; set; }
        public string RegexWordCharacters { get; set; }
        public string Direction { get; set; }
    }
}