#region License
// PublicProfileModel.cs is part of ReadingTool.Models
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
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Create.User
{
    public class PublicProfileModel
    {
        public string Username { get; private set; }

        [DisplayName("Who can see your profile?")]
        public PublicProfileAvailability Availability { get; set; }

        [DisplayName("Show your statistics?")]
        [Help("This is simply the languages you are using and the number of known words in each. The system language name will be used unless it is not set, " +
            "in that case your name for the language is used instead.")]
        public bool ShowStats { get; set; }

        [DisplayName("Show your native language?")]
        [Help("If you have your native/main language set in your profile, check this box to display it in your public profile.")]
        public bool ShowNativeLanguage { get; set; }

        [Help("Your location, this can be anything.")]
        [StringLength(25)]
        public string Location { get; set; }

        [DisplayName("Twitter URL")]
        [Help("If you have a Twitter account, fill in your public URL here.")]
        [StringLength(50)]
        public string TwitterUrl { get; set; }

        [DisplayName("Your homepage URL")]
        [Help("If you have a blog or personal website, fill in the URL here.")]
        [StringLength(50)]
        public string WebsiteUrl { get; set; }

        [DisplayName("About Me")]
        [Help("Anything you want to say about yourself. (max 2000 characters)")]
        [StringLength(2000)]
        public string AboutMe { get; set; }
    }
}