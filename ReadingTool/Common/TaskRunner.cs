#region License
// TaskRunner.cs is part of ReadingTool
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

using System;
using System.Reflection;
using System.Web.Mvc;
using MongoDB.Driver;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Tasks;
using StructureMap;
using log4net;

namespace ReadingTool.Common
{
    public sealed class TaskRunner
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _assemblyName = SystemSettings.Instance.Values.Tasks.AssemblyName;

        /// <summary>
        /// Make sure to check for nulls
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static ITask InstatiateTask(string className)
        {
            try
            {
                var qualifiedName = Assembly.CreateQualifiedName(_assemblyName, _assemblyName + "." + className);
                ITask taskClass = (ITask)Activator.CreateInstance(Type.GetType(qualifiedName, true));
                return taskClass;
            }
            catch
            {
                return null;
            }
        }

        public static SystemTaskResult Run(SystemTask task)
        {
            if(task == null)
            {
                return new SystemTaskResult { Message = "Task is null" };
            }

            if(!task.IsActive)
            {
                return new SystemTaskResult { Message = "Task is inactive" };
            }

            var taskService = DependencyResolver.Current.GetService<ITaskService>();

            if(taskService == null)
                return new SystemTaskResult() { Message = "Unable to resolve ITaskService", Success = false };

            SystemTaskResult taskResult = new SystemTaskResult() { Message = "Unknown", Success = false };

            try
            {
                var taskClass = InstatiateTask(task.ClassName);

                if(taskClass == null)
                {
                    taskResult.Message = "Class did not instantiate";
                    return taskResult;
                }

                taskResult = taskClass.Run(ObjectFactory.GetInstance<MongoDatabase>());

                if(!taskResult.Success)
                {
                    task.ConsecutiveFailures++;

                    if(task.ConsecutiveFailures > task.MaximumFailures)
                    {
                        Logger.InfoFormat("Task {0}: consecutive failures ({1}) > maximum failures ({2})", task.Name, task.ConsecutiveFailures, task.MaximumFailures);
                    }
                }
                else
                {
                    task.ConsecutiveFailures = 0;

                    if(Logger.IsDebugEnabled)
                    {
                        Logger.DebugFormat("Task {0} successfully completed", task.Name);
                    }
                }
            }
            catch(Exception e)
            {
                task.ConsecutiveFailures++;
                Logger.Error(e);

                if(task.ConsecutiveFailures > task.MaximumFailures)
                {
                    Logger.InfoFormat("Task {0}: consecutive failures ({1}) > maximum failures ({2})", task.Name, task.ConsecutiveFailures, task.MaximumFailures);
                }
            }
            finally
            {
                task.LastRunDate = DateTime.Now;
                taskService.Save(task);
            }

            return taskResult;
        }
    }
}