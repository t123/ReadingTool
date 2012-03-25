#region License
// LanguageModel.cs is part of ReadingTool.API
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
    public class LanguageModel
    {
        public string LanguageId { get; set; }
        public string Name { get; set; }
        public string TranslateUrl { get; set; }
        public bool IsRtlLanguage { get; set; }
        public bool HasRomanisationField { get; set; }
        public bool RemoveSpaces { get; set; }
        public string ModalBehaviour { get; set; }
        public string PunctuationRegEx { get; set; }
        public string SentenceEndRegEx { get; set; }
        public string Colour { get; set; }
        public bool KeepFocus { get; set; }
        public string Created { get; set; }
        public string Modified { get; set; }
    }
}