#region License
// MediaControlModel.cs is part of ReadingTool.Models
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
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.User
{
    public class MediaControlModel
    {
        [DisplayName("Enable controls")]
        [Help("When media controls are enabled you can control the media player with the keys defined below. This only applies when the text popup is not open.")]
        public bool IsEnabled { get; set; }

        [DisplayName("Pause audio on text popup")]
        [Help("Automatically pause the audio or video when the text popup opens. It will resume when the popup is closed.")]
        public bool AutoPause { get; set; }

        [DisplayName("Seconds to rewind/fast forward")]
        [Help("This is the number of seconds to fast forward and rewind the audio/video when the fast forward or rewind button is clicked.")]
        public decimal SecondsToRewind { get; set; }

        [DisplayName("Rewind to Beginning")]
        public int? RewindToBeginning { get; set; }

        [DisplayName("Rewind")]
        [Help("This rewinds by the number of seconds defined in <u>seconds to rewind</u>")]
        public int? Rewind { get; set; }

        [DisplayName("Play/pause audio")]
        public int? PlayPause { get; set; }

        [DisplayName("Stop audio")]
        public int? Stop { get; set; }

        [DisplayName("Fast forward")]
        [Help("This fast forwards by the number of seconds defined in <u>seconds to rewind</u>")]
        public int? FastForward { get; set; }

        public MediaControlModel()
        {
            SecondsToRewind = 4;
        }
    }
}