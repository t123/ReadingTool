#region License
// ImportLanguageModel.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Web;

namespace ReadingTool.Areas.Admin.Models
{
    public class ImportLanguageModel
    {
        public HttpPostedFileBase File { get; set; }
        public int CodeColumnNo { get; set; }
        public int LanguageNameColumnNo { get; set; }

        public ImportLanguageModel()
        {
            CodeColumnNo = 0;
            LanguageNameColumnNo = 6;
        }
    }
}