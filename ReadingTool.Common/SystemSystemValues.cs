#region License
// SystemSystemValues.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Helpers;

namespace ReadingTool.Common
{
    [BsonIgnoreExtraElements]
    public sealed class SystemSystemValues
    {
        [BsonId]
        public string SettingsKey { get; set; }

        public ValueFormats Formats { get; set; }
        public ValueSmtp Smtp { get; set; }
        public ValueSite Site { get; set; }
        public ValueTasks Tasks { get; set; }
        public ValueData UserData { get; set; }
        public ValueMisc Misc { get; set; }
        public ValueSecurity Security { get; set; }

        public SystemSystemValues(string settingsKey)
        {
            SettingsKey = settingsKey ?? "default";

            Security = new ValueSecurity();
            Formats = new ValueFormats();
            Smtp = new ValueSmtp();
            Site = new ValueSite();
            Tasks = new ValueTasks();
            UserData = new ValueData();
            Misc = new ValueMisc();
        }

        [BsonIgnoreExtraElements]
        public class ValueFormats
        {
            public string LongDateFormat;
            public string ShortDateFormat;
            public string Time24Format;
            public string Time12Format;

            public ValueFormats()
            {
                LongDateFormat = "dd MMM yyyy";
                ShortDateFormat = "dd/MM/yy";
                Time24Format = "HH:mm";
                Time12Format = "hh:mm";
            }
        }

        [BsonIgnoreExtraElements]
        public class ValueSmtp
        {
            public string Server;
            public string Username;
            public string Password;
            public int? Port;

            public ValueSmtp()
            {
                Server = Username = Password = string.Empty;
            }
        }

        [BsonIgnoreExtraElements]
        public class ValueSite
        {
            public string BasePath { get; set; }
            public string NiceName { get; set; }
            public string Domain { get; set; }
            public string ContactEmail { get; set; }
            public string AdminNotifyEmail { get; set; }
            public string AdminUsername { get; set; }
            public string DefaultFromEmail { get; set; }
            public string DefaultReplyEmail { get; set; }
            public string BugReportingUrl { get; set; }
            public string SourceCodeUrl { get; set; }
            public string ApiUrl { get; set; }
            public string Analytics { get; set; }
            public string ScriptVersioning { get; set; }
            public int MaxWordsParsingException { get; set; }

            public ValueSite()
            {
            }
        }

        [BsonIgnoreExtraElements]
        public class ValueTasks
        {
            public string AssemblyName { get; set; }

            public ValueTasks()
            {
                AssemblyName = "ReadingTool.Tasks";
            }
        }

        [BsonIgnoreExtraElements]
        public class ValueSecurity
        {
            public string SharedSecret { get; set; }

            public ValueSecurity()
            {
                SharedSecret = PasswordHelper.CreateRandomString(50, PasswordHelper.AllowedCharacters.AlphaNumericSpecial);
            }
        }

        [BsonIgnoreExtraElements]
        public class ValueData
        {
            public bool AllowAccountImport { get; set; }
            public bool VerifyHash { get; set; }

            public ValueData()
            {
                AllowAccountImport = true;
                VerifyHash = true;
            }
        }

        public class ValueMisc
        {
            private Dictionary<string, object> _values;

            public ValueMisc()
            {
                _values = new Dictionary<string, object>();
            }

            public T TryGetSetting<T>(string key, T defaultValue)
            {
                object value;
                if(_values.TryGetValue(key, out value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return defaultValue;
            }
        }
    }
}
