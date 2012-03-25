#region License
// Settings.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Configuration;
using ReadingTool.Common.Keys;
using ReadingTool.Services;
using StructureMap;

namespace ReadingTool.Common
{
    public sealed class SystemSettings
    {
        private static volatile SystemSettings _settings = null;
        private static readonly object _lock = new object();
        private SystemSystemValues _values;
        public SystemSystemValues Values
        {
            get { return _values; }
        }

        private SystemSettings()
        {
            if(_values == null)
            {
                var settingsKeys = ConfigurationManager.AppSettings[CacheKeys.SETTINGS_KEY];
                var service = ObjectFactory.GetInstance<ISystemSettingsService>();
                var first = service.Settings(settingsKeys);

                if(first == null)
                {
                    _values = new SystemSystemValues(settingsKeys);
                    service.Save(_values);
                }
                else
                {
                    _values = first;
                    service.Save(_values);
                }
            }
        }

        public void Import(SystemSystemValues values)
        {
            var service = ObjectFactory.GetInstance<ISystemSettingsService>();
            _values = values;
            service.Save(_values);
            Reload();
        }

        public void Reload()
        {
            if(_settings != null)
            {
                lock(_lock)
                {
                    _settings = null;
                }
            }
        }

        public static SystemSettings Instance
        {
            get
            {
                if(_settings == null)
                {
                    lock(_lock)
                    {
                        if(_settings == null)
                        {
                            _settings = new SystemSettings();
                        }
                    }
                }

                return _settings;
            }
        }
    }
}
