#region License
// ParsingWord.cs is part of ReadingTool.Entities
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
// Copyright (C) 2012 Travis Watt
#endregion

using MongoDB.Bson;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class ParsingWord
    {
        public ObjectId WordId { get; set; }
        public string WordPhrase { get; set; }
        public string WordPhraseLower { get; set; }
        public WordState State { get; set; }
        public string Definition { get; set; }
        public string BaseWord { get; set; }
        public string Romanisation { get; set; }
        public int Length { get; set; }
        public string FullDefinition
        {
            get { return string.Join("\n", new string[] { BaseWord, Romanisation, Definition }).Trim(); }
        }
    }
}