#region License
// TextImport.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

namespace ReadingTool.Entities
{
    public class TextImport
    {
        public class JsonDefaults
        {
            public string L1LanguageName { get; set; }
            public string L2LanguageName { get; set; }
            public string CollectionName { get; set; }
            public bool? AutoNumberCollection { get; set; }
            public int? StartCollectionWithNumber { get; set; }

            public JsonDefaults()
            {
                L1LanguageName = string.Empty;
                L2LanguageName = string.Empty;
                CollectionName = string.Empty;
                StartCollectionWithNumber = null;
                AutoNumberCollection = false;
            }
        }

        public class JsonTextItem
        {
            public string L1LanguageName { get; set; }
            public string L2LanguageName { get; set; }
            public string Title { get; set; }
            public string AudioUrl { get; set; }
            public string CollectionName { get; set; }
            public int? CollectionNo { get; set; }
            public string L1Text { get; set; }
            public string L2Text { get; set; }
        }

        public JsonDefaults Defaults { get; set; }
        public JsonTextItem[] Items { get; set; }
    }
}
