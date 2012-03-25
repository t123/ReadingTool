#region License
// PublicProfileViewModel.cs is part of ReadingTool.Models
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

using System;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.View.User
{
    public class PublicProfileViewModel
    {
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string EmailAddressMD5 { get; private set; }
        public string Fullname { get; private set; }
        public TimeSpan Joined { get; private set; }
        public string NativeLanguage { get; private set; }
        public bool ReceiveMessages { get; set; }
        public PublicProfileAvailability Availability { get; set; }
        public bool ShowStats { get; set; }
        public bool ShowNativeLanguage { get; set; }
        public string Location { get; set; }
        public string TwitterUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string AboutMe { get; set; }
    }
}