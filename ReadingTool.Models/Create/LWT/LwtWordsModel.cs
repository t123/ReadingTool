#region License
// LwtWordsModel.cs is part of ReadingTool.Models
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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Create.LWT
{
    public class LwtWordsImportModel
    {
        [Required(ErrorMessage = "Please upload a file")]
        [DisplayName("File to upload")]
        [Help("This is the file created by the <strong>Export ALL terms (TSV)</strong> function in LWT.")]
        public HttpPostedFileBase File { get; set; }
    }

    public class LwtWordModel
    {
        public ObjectId LanguageId { get; set; }
        public string Term { get; set; }
        public string Translation { get; set; }
        public string[] Tags { get; set; }
        public string Romanisation { get; set; }
        public string Sentence { get; set; }
        private int _box;
        public int Box
        {
            get { return _box; }
            set
            {
                _box = value;
                switch(value)
                {
                    case 98: WordState = WordState.Ignored; break;
                    case 99: WordState = WordState.Known; _box = 9; break;
                    default: WordState = WordState.Unknown; break;
                }
            }
        }

        public WordState WordState { get; set; }
    }
}