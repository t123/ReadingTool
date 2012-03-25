#region License
// MultiTextPartModel.cs is part of ReadingTool.Models
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
using MongoDB.Bson;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.Text
{
    public class MultiTextPartModel
    {
        [DisplayName("Collection Number")]
        [Help("You don't have to number the texts. If you do number them, you can jump between the next and previous texts in the same collection.")]
        public int? CollectionNo { get; set; }

        [Required]
        [Help("The title of the text.")]
        public string Title { get; set; }

        [DisplayName("Audio URL")]
        [DataType(DataType.Url)]
        [Help("The URL of the text, it must start with <u>http://</u><br/>Please do not hotlink to other peoples files without permission. " +
              "You can store your files on your own host or places like DropBox. If you always use the same computer, it may be better to install " +
              "a webserver on your computer and refer to them with http://localhost")]
        public string AudioUrl { get; set; }

        [DisplayName("Share URL with groups?")]
        [Help("If you want to share your texts with groups, but not the URL of the video check this box.")]
        public bool ShareUrl { get; set; }

        [Help("Texts like Japanese do not work since they have no spaces. You can do this yourself or you can choose one of the parsers " +
              "to split it for you.")]
        [DisplayName("Parse With")]
        public ObjectId? ParseWith { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        [DisplayName("Parallel Text")]
        [DataType(DataType.MultilineText)]
        [Help("If you have a parallel text you can put it here and then you can choose between reading in normal mode and in parallel. " +
              "The texts <u>must</u> line up exactly, or more correctly the number of line breaks (\\n) must be the same.")]
        public string ParallelText { get; set; }

        [DisplayName("Is the parallel text RTL?")]
        [Help("Check this box if your parallel text is a right to left language.")]
        public bool ParallelIsRtl { get; set; }
    }
}