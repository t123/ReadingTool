#region License
// TextFromFileModel.cs is part of ReadingTool.Models
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
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.Text
{
    public class TextFromFileModel
    {
        [Required(ErrorMessage = "Please upload a file")]
        [DisplayName("File with texts")]
        [Help("Remember the file must be UTF-8. See the sample for an example how to layout out your upload file.")]
        public HttpPostedFileBase File { get; set; }
    }
}