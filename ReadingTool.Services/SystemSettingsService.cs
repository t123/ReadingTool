#region License
// SystemSettingsService.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Linq;
using FluentMongo.Linq;
using MongoDB.Driver;
using ReadingTool.Common;

namespace ReadingTool.Services
{
    public interface ISystemSettingsService
    {
        SystemSystemValues Settings(string settingsKey);
        void Save(SystemSystemValues settings);
    }

    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly MongoDatabase _db;

        public SystemSettingsService(MongoDatabase db)
        {
            _db = db;
        }

        public SystemSystemValues Settings(string settingsKey)
        {
            settingsKey = settingsKey ?? "default";
            return _db.GetCollection<SystemSystemValues>(Collections.SystemSettings)
                .AsQueryable()
                .FirstOrDefault(x => x.SettingsKey == settingsKey);
        }

        public void Save(SystemSystemValues settings)
        {
            _db.GetCollection(Collections.SystemSettings).Save(settings);
        }
    }
}
