#region License
// Manager.cs is part of ReadingTool.TaskManager
// 
// ReadingTool.TaskManager is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.TaskManager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.TaskManager. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Reflection;
using ReadingTool.Common;
using ReadingTool.Services;
using ReadingTool.TaskManager.DependencyResolution;
using StructureMap;
using log4net;

namespace ReadingTool.TaskManager
{
    public sealed class Manager
    {
        private static volatile object _lock = new object();
        private static volatile Manager _manager = null;
        private readonly ITaskService _taskService;
        private readonly ISystemSettingsService _settingsService;
        private readonly SystemSystemValues _settings;
        private static readonly log4net.ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Manager()
        {
            IoC.Initialize();
            _taskService = ObjectFactory.GetInstance<ITaskService>();
            _settingsService = ObjectFactory.GetInstance<ISystemSettingsService>();
            _settings = _settingsService.Settings(null);
        }

        public void Run()
        {
            var tasksToRun = _taskService.FindAllRunnableTasks();

            foreach(var task in tasksToRun)
            {
                Logger.InfoFormat("Task {0} must execute", task.Name);
                var result = TaskRunner.Run(task, _settings.Tasks.AssemblyName);

                if(!result.Success)
                {
                    Logger.Info(result);
                }
            }
        }

        public static Manager Instance
        {
            get
            {
                if(_manager == null)
                {
                    lock(_lock)
                    {
                        if(_manager == null)
                        {
                            _manager = new Manager();
                        }
                    }
                }

                return _manager;
            }
        }
    }
}
