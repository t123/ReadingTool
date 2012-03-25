#region License
// Lwt.cs is part of ReadingTool.Entities
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

namespace ReadingTool.Entities.LWT
{
    public class Lwt
    {
        public string JsonData { get; set; }
        public bool TestMode { get; set; }
        public string Errors { get; set; }
        public string MediaUrl { get; set; }
        public string Hostname { get; set; }
        public int? Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DbName { get; set; }
        public string UserConnectionString { get; set; }
        public bool Ping { get; set; }
        public bool TagsToCollections { get; set; }
        public string NumberCollectionRegEx { get; set; }
        public bool UseRomanisationAsBaseWord { get; set; }
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
    }
}