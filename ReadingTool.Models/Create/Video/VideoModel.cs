#region License
// VideoModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Create.Video
{
    public class VideoModel
    {
        public ObjectId ItemId { get; set; }

        [Required]
        [DisplayName("Language")]
        [Help("The language of the video.")]
        public ObjectId LanguageId { get; set; }

        [DisplayName("Collection Name")]
        [Help("Use this option to group videos into collections.")]
        public string CollectionName { get; set; }

        [DisplayName("Collection Number")]
        [Help("You don't have to number the videos. If you do number them, you can jump between the next and previous videos in the same collection.")]
        public int? CollectionNo { get; set; }

        [Required]
        [Help("The title of the video.")]
        public string Title { get; set; }

        [DisplayName("Video URL")]
        [DataType(DataType.Url)]
        [Help("The URL of the video, currently only MPEG4 is supported. It must start with <u>http://</u><br/>Please do not hotlink to other peoples files without permission. " +
            "You can store your files on your own host or places like DropBox. If you always use the same computer, it may be better to install " +
            "a webserver on your computer and refer to them with http://localhost")]
        public string Url { get; set; }

        [Help("Any tags for your texts. Tags are comma separated.")]
        public string Tags { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("L1 Subtitles")]
        [Help("The subtitles for you video in SRT format.")]
        public string L1Text { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("L2 Subtitles")]
        [Help("The corresponding subtitles for you video in SRT format. Unlike texts they do not have to match up, since the display " +
            "is based on the times in the file.")]
        public string L2Text { get; set; }
    }
}