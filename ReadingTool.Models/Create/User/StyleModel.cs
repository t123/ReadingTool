#region License
// StyleModel.cs is part of ReadingTool.Models
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
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.User
{
    public class StyleModel
    {
        [Help("Use this if you'd like to use your own external stylesheet for reading.")]
        [DisplayName("Reading CSS Url")]
        public string ReadingCssUrl { get; set; }

        [Help("Use this if you'd like to use your own external stylesheet for watching.")]
        [DisplayName("Watching CSS Url")]
        public string WatchingCssUrl { get; set; }

        [Help("The background color for new words in the format #RRGGBB or #RGB.")]
        [DisplayName("Not Seen Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string NotSeen { get; set; }

        [Help("The background colour for new words in the format #RRGGBB.")]
        [DisplayName("Known Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string Known { get; set; }

        [Help("The background colour for unknown words in the format #RRGGBB or #RGB..")]
        [DisplayName("Unknown Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string Unknown { get; set; }

        [Help("The background colour for ignored words in the format #RRGGBB or #RGB..")]
        [DisplayName("Ignored Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string Ignored { get; set; }

        [Help("The background colour for the margins in the format #RRGGBB or #RGB..")]
        [DisplayName("Text Area Background Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string TextAreaBackground { get; set; }

        [Help("The background colour for the reading area in the format #RRGGBB or #RGB..")]
        [DisplayName("Reading Area Background Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string TextContentBackground { get; set; }

        [Help("The font for the text.")]
        [DisplayName("Text Font")]
        public string TextContentFont { get; set; }

        [Help("The colour for the text in the format #RRGGBB or #RGB..")]
        [DisplayName("Text Colour")]
        [AltRegularExpression("^#?(([a-fA-F0-9]){3}){1,2}$", ErrorMessage = "Please use the form #RRGGBB or #RGB.")]
        public string TextContentColour { get; set; }

        [Help("The size of the text in pixels.")]
        [Range(6, 50)]
        [DisplayName("Text Size")]
        public int? TextSize { get; set; }

        [Help("This controls the spacing between lines. The height is in pixels.")]
        [Range(1, 50)]
        [DisplayName("Line Height")]
        public int? LineHeight { get; set; }

        public StyleModel()
        {
        }
    }
}