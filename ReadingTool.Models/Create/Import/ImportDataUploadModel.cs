#region License
// ImportDataUploadModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Create.Import
{
    public class ImportDataUploadModel
    {
        [Required(ErrorMessage = "Please upload a file")]
        [DisplayName("File to upload")]
        [Help("This is the file JSON file that was exported with the export function.")]
        public HttpPostedFileBase File { get; set; }
    }
}