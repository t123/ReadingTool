#region License
// DictionaryViewModel.cs is part of ReadingTool.Site
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

namespace ReadingTool.Site.Models.Languages
{
    public class DictionaryViewModel
    {
        public long DictionaryId { get; set; }
        public long LanguageId { get; set; }
        public string Name { get; set; }
        public string Encoding { get; set; }
        public string WindowName { get; set; }
        public string Url { get; set; }
        public bool Sentence { get; set; }
        public bool AutoOpen { get; set; }
    }
}