#region License
// Style.cs is part of ReadingTool.Entities
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
namespace ReadingTool.Entities
{
    public class Style
    {
        public string ReadingCssUrl { get; set; }
        public string WatchingCssUrl { get; set; }
        public string NotSeen { get; set; }
        public string Known { get; set; }
        public string Unknown { get; set; }
        public string Ignored { get; set; }
        public string TextAreaBackground { get; set; }
        public string TextContentBackground { get; set; }
        public string TextContentFont { get; set; }
        public string TextContentColour { get; set; }
        public int? TextSize { get; set; }
        public int? LineHeight { get; set; }

        public Style()
        {
        }

        public string Css
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                                     TextAreaBackgroundAsCss,
                                     TextContentBackgroundAsCss,
                                     TextContentFontAsCss,
                                     TextContentColourAsCss,
                                     TextSizeAsCss,
                                     LineHeightAsCss,
                                     KnownAsCss,
                                     UnknownAsCss,
                                     IgnoredAsCss,
                                     NotSeenAsCss
                    );
            }
        }

        public string NotSeenAsCss { get { return string.IsNullOrEmpty(NotSeen) ? "" : string.Format("#textContent span.nsx {{ background-color: {0} !important; }}", NotSeen); } }
        public string KnownAsCss { get { return string.IsNullOrEmpty(Known) ? "" : string.Format("#textContent span.knx {{ background-color: {0} !important; }}", Known); } }
        public string UnknownAsCss { get { return string.IsNullOrEmpty(Unknown) ? "" : string.Format("#textContent span.nkx {{ background-color: {0} !important; }}", Unknown); } }
        public string IgnoredAsCss { get { return string.IsNullOrEmpty(Ignored) ? "" : string.Format("#textContent span.igx {{ background-color: {0} !important; }}", Ignored); } }
        public string TextAreaBackgroundAsCss { get { return string.IsNullOrEmpty(TextAreaBackground) ? "" : string.Format("#textArea {{ background-color: {0} !important; }}", TextAreaBackground); } }
        public string TextContentBackgroundAsCss { get { return string.IsNullOrEmpty(TextContentBackground) ? "" : string.Format("#textContent {{ background-color: {0} !important; }}", TextContentBackground); } }
        public string TextContentFontAsCss { get { return string.IsNullOrEmpty(TextContentFont) ? "" : string.Format("#textContent {{ font-family: {0} !important; }}", TextContentFont); } }
        public string TextContentColourAsCss { get { return string.IsNullOrEmpty(TextContentColour) ? "" : string.Format("#textContent {{ color: {0} !important; }}", TextContentColour); } }
        public string TextSizeAsCss { get { return TextSize == null ? "" : string.Format("#textContent {{ font-size: {0}px !important; }}", TextSize); } }
        public string LineHeightAsCss { get { return LineHeight == null ? "" : string.Format("#textContent span {{ line-height: {0}px; }}", LineHeight); } }
    }
}