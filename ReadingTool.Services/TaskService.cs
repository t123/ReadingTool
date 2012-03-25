#region License
// TaskService.cs is part of ReadingTool.Services
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ITaskService
    {
        void Save(SystemTask task);
        IEnumerable<SystemTask> FindAll();
        DateTime? NextRunDate(SystemTask task);
        SystemTask FindOne(ObjectId taskId);
        void Delete(SystemTask task);
        IEnumerable<SystemTask> FindAllRunnableTasks();
        //void SaveResult(SystemTask task, SystemTaskResult taskResult);
        //IEnumerable<BsonDocument> FindAllResults();
        //void ResetAllTasks();
    }

    public class TaskService : ITaskService
    {
        private readonly MongoDatabase _db;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Services");

        public TaskService(MongoDatabase db)
        {
            _db = db;
        }

        public void Save(SystemTask task)
        {
            if(!task.IsRunning)
            {
                task.NextRunDate = NextRunDate(task);
            }

            _db.GetCollection(Collections.Tasks).Save(task);
        }

        public IEnumerable<SystemTask> FindAll()
        {
            return _db.GetCollection<SystemTask>(Collections.Tasks).AsQueryable().OrderByDescending(x => x.IsActive);
        }

        public DateTime? NextRunDate(SystemTask task)
        {
            if(task.Schedule == TaskSchedule.Once && task.LastRunDate.HasValue) return null;
            DateTime nextRun;

            switch(task.Schedule)
            {
                case TaskSchedule.Hourly:
                    nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, task.Minute ?? 0, 0);
                    if(nextRun.AddMinutes(1) < DateTime.Now) nextRun = nextRun.AddHours(1);
                    return nextRun;
                    break;

                case TaskSchedule.FixedTime:
                case TaskSchedule.Once:
                    nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, task.Hour ?? 23, task.Minute ?? 59, 0);
                    if(nextRun.AddMinutes(1) < DateTime.Now) nextRun = nextRun.AddDays(1);
                    return nextRun;
                    break;

                case TaskSchedule.Periodically:
                    if(task.LastRunDate == null)
                    {
                        return DateTime.Now.AddMinutes(task.Minutes ?? 60);
                    }
                    else
                    {
                        return task.LastRunDate.Value.AddMinutes(task.Minutes ?? 60);
                    }
                    break;

                default:
                    throw new NotSupportedException("Task has invalid schedule???");
            }
        }

        public SystemTask FindOne(ObjectId taskId)
        {
            return _db.GetCollection<SystemTask>(Collections.Tasks).AsQueryable().FirstOrDefault(x => x.SystemTaskId == taskId);
        }

        public void Delete(SystemTask task)
        {
            if(task == null) return;
            _db.GetCollection(Collections.Tasks).Remove(Query.EQ("_id", task.SystemTaskId));
        }

        public IEnumerable<SystemTask> FindAllRunnableTasks()
        {
            var now = DateTime.Now;

            return _db.GetCollection<SystemTask>(Collections.Tasks)
                .AsQueryable()
                .Where(x => x.IsActive && !x.IsRunning && x.NextRunDate < now)
                .OrderBy(x => x.NextRunDate);
        }

        //public void SaveResult(SystemTask task, SystemTaskResult taskResult)
        //{
        //    StringBuilder exception = new StringBuilder();
        //    var currentException = taskResult.Exception;
        //    while(currentException != null)
        //    {
        //        exception.AppendFormat("\n\n{0}", currentException.Message);
        //        currentException = currentException.InnerException;
        //    }

        //    BsonDocument document = new BsonDocument();
        //    document.Add("Created", DateTime.Now);
        //    document.Add("SystemTaskId", task.SystemTaskId);
        //    document.Add("Success", taskResult.Success);
        //    document.Add("Message", taskResult.Message);
        //    document.Add("Exception", exception.ToString());

        //    _db.GetCollection(Collections.TaskResults).Save(document);
        //}

        //public IEnumerable<BsonDocument> FindAllResults()
        //{
        //    return _db.GetCollection(Collections.TaskResults).FindAll();
        //}

        //public void ResetAllTasks()
        //{
        //    _db.GetCollection(Collections.Tasks)
        //        .Update(null, Update.Set("IsRunning", false));
        //}
    }
}
