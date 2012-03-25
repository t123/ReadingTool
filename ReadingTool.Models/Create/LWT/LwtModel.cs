﻿#region License
// LwtModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Create.LWT
{
    public class LwtModel
    {
        public string Errors { get; set; }

        [Help("If you use the <u>choose from folder option in LWT</u>, your audio is probably stored with the relative URL. You shoud " +
              "fill in this field to change the URL to an absolute URL. Absolute URLs are not affected. For example if you hosted " +
              "LWT at http://lwt.example.org/ you should fill that in.")]
        [DisplayName("Default Media Url")]
        public string MediaUrl
        {
            get { return _mediaUrl; }
            set
            {
                _mediaUrl = value ?? "";
                if(!_mediaUrl.EndsWith("/")) _mediaUrl += "/";
            }
        }
        private string _mediaUrl;

        [Help("This tests to make sure your data can be successfully retrieved.")]
        [DisplayName("Test Mode")]
        public bool TestMode { get; set; }

        [Help("This is the $server parameter in <strong>connect.inc.php</strong>.")]
        public string Hostname { get; set; }

        [Help("You may leave this blank unless you need to change the default port.")]
        public int? Port { get; set; }

        [Help("This is the $userid parameter in <strong>connect.inc.php</strong>.")]
        public string Username { get; set; }

        [Help("This is the $passwd parameter in <strong>connect.inc.php</strong>.")]
        public string Password { get; set; }

        [Help("This is the $dbname parameter in <strong>connect.inc.php</strong>.")]
        [DisplayName("Database Name")]
        public string DbName { get; set; }

        [Help("<strong>Advanced:</strong> You may specify the MySql connection string directly here.")]
        [DisplayName("Connection String")]
        public string UserConnectionString { get; set; }

        [Help("Check this box if you just want to test your settings without importing any data.")]
        public bool Ping { get; set; }

        [Help("Check this box if you just want to use your tags as a collection name for your texts. If you have multiple tags only the first tag " +
              "will be used.")]
        [DisplayName("Use tag for collection name?")]
        public bool TagsToCollections { get; set; }

        [Help("If you used the romanisation field to hold the base word value, check this box.")]
        [DisplayName("Romanisation field is base word?")]
        public bool UseRomanisationAsBaseWord { get; set; }

        [Help("Leave this box blank if you don't want to automatically number your text titles. Otherwise the regular expression is used " +
              "to match the number.")]
        [DisplayName("Number texts by title")]
        public string NumberCollectionRegEx { get; set; }
        public string ConnectionString
        {
            get
            {
                string connectionString = UserConnectionString;

                if(string.IsNullOrEmpty(connectionString))
                {
                    if(Port == null) Port = 3306;
                    connectionString = string.Format(
                        "Server={0};Port={1};Database={2};Uid={3};Pwd={4};",
                        Hostname,
                        Port,
                        DbName,
                        Username,
                        Password);
                }

                return connectionString;
            }
        }

        public LwtModel()
        {
            TestMode = true;
            NumberCollectionRegEx = @"^[\d]+|[\d]+$|[\w]*[\d+]:|[\w]*[\d+]-|[\w]*[\d+] -";
        }
    }
}