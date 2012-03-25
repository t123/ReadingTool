#region License
// ProfileModel.cs is part of ReadingTool.Models
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;

namespace ReadingTool.Models.Create.User
{
    public class ProfileModel
    {
        public ObjectId UserId { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }

        [DisplayName("Display Name")]
        [Help("This is displayed in addition to your username.")]
        [StringLength(25)]
        public string DisplayName { get; set; }

        [DisplayName("Email Address")]
        [AltRegularExpression(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$", ErrorMessage = "Please enter a valid email address")]
        [Help("This is not required and never displayed, but is needed if you forget your password or if you want to display your gravatar.")]
        [StringLength(50)]
        public string EmailAddress { get; set; }
        public string EmailAddressMD5 { get; set; }

        [DisplayName("Native/Main Language")]
        [StringLength(25)]
        [Help("What is your home language, or the language you most commonly use.")]
        [Remote("ValidateNativeLanguageName", "RemoteValidator", HttpMethod = "POST")]
        public string NativeLanguage { get; set; }

        [DataType(DataType.Password)]
        [Help("If you want to change your password, fill in your current password here.")]
        [DisplayName("Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        [Help("If you want to change your password, fill in the new password here.")]
        public string UnencryptedPassword { get; set; }

        [Compare("UnencryptedPassword")]
        [DataType(DataType.Password)]
        [DisplayName("Confirm New Password")]
        public string ConfirmPassword { get; set; }

        [DisplayName("Enable Messages?")]
        [Help("Check this box if you want send or allow other users to be able to send you messages. These are internal messages and not linked to your email address.")]
        public bool ReceiveMessages { get; set; }

        [DisplayName("Share your word definitions?")]
        [Help("If you want to share your word definitions with other people, check this box. This shares the base word, romanisation, sentence and defintion " +
              "of your words. No other data is shared. You can only see other peoples definitions of words if you share your own definitions.")]
        public bool ShareWords { get; set; }

        public string Fullname
        {
            get
            {
                if(string.IsNullOrEmpty(DisplayName)) return Username;
                return Username + " (" + DisplayName + ")";
            }
        }
    }
}