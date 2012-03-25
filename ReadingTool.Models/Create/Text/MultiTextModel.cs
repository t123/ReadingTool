#region License
// MultiTextModel.cs is part of ReadingTool.Models
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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.Text
{
    public class MultiTextModel
    {
        [Required]
        [DisplayName("Language")]
        [Help("This applies to all the text parts added")]
        public ObjectId LanguageId { get; set; }

        [DisplayName("Collection Name")]
        [Help("Use this option to group texts into collections. All the texts added will be in this collection")]
        public string CollectionName { get; set; }

        [Help("Any tags for your texts. Tags are comma separated. All the texts add will have these tags")]
        public string Tags { get; set; }

        public IList<MultiTextPartModel> Parts { get; set; }

        public MultiTextModel()
        {
            Parts = new List<MultiTextPartModel>();
        }
    }
}