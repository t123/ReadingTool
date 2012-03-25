#region License
// UserDictionaryModel.cs is part of ReadingTool.Models
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

using System.ComponentModel.DataAnnotations;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.Language
{
    public class UserDictionaryModel
    {
        [Required]
        [Help("This is your name for the dictionary")]
        public string Name { get; set; }

        [Required]
        [Help("This is the name of the window or tab that your dictonary opens in. Dictionaries with the same window name will open " +
            "in the same window/tab. You can use this to group your dictionaries. Once a window is open, this dictiomary will continue to open in " +
            "that same window, so you can position the window and leave it. If you want all your dictionaries to open in the same window simply " +
            "use the same name for each")]
        [Display(Name = "Window Name")]
        public string WindowName { get; set; }

        [Required]
        [Help("This is the URL for your dictionary. You can use [[word]] to send the current word. For example a link to Word Reference English to French " +
            "would be www.wordreference.com/enfr/[[word]]")]
        public string Url { get; set; }
    }
}