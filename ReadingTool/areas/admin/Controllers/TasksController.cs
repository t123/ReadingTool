#region License
// TasksController.cs is part of ReadingTool
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

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Areas.Admin.Models;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class TasksController : BaseController
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [AutoMap(typeof(IEnumerable<SystemTask>), typeof(IEnumerable<SystemTaskModel>))]
        public ActionResult Index()
        {
            return View(_taskService.FindAll());
        }

        [HttpGet]
        public ActionResult AddTask()
        {
            return View();
        }

        //public ActionResult TaskResults()
        //{
        //    ViewBag.Tasks = _taskService.FindAll().ToDictionary(x => x.SystemTaskId, x => Mapper.Map<SystemTask, SystemTaskModel>(x));
        //    return View(_taskService.FindAllResults());
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTask(SystemTaskModel model)
        {
            if(TaskRunner.InstatiateTask(model.ClassName) == null)
            {
                ModelState.AddModelError("ClassName", string.Format("The class name was not found in the assembly {0}", SystemSettings.Instance.Values.Tasks.AssemblyName));
            }

            if(ModelState.IsValid)
            {
                var task = Mapper.Map<SystemTaskModel, SystemTask>(model);
                _taskService.Save(task);

                return this.RedirectToAction(x => x.Index()).Success("Task added");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult EditTask(string id)
        {
            var task = _taskService.FindOne(new ObjectId(id));

            if(task == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Task not found");
            }

            return View(Mapper.Map<SystemTask, SystemTaskModel>(task));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTask(string id, SystemTaskModel model)
        {
            var task = _taskService.FindOne(new ObjectId(id));

            if(task == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Task not found");
            }

            if(TaskRunner.InstatiateTask(task.ClassName) == null)
            {
                ModelState.AddModelError("ClassName", string.Format("The class name was not found in the assembly {0}", SystemSettings.Instance.Values.Tasks.AssemblyName));
            }

            if(ModelState.IsValid)
            {
                TryUpdateModel(task, new[] { "Name", "ClassName", "IsActive", "Schedule", "Minutes", "Hour", "Minute" });
                _taskService.Save(task);

                return this.RedirectToAction(x => x.Index()).Success("Task updated");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var task = _taskService.FindOne(new ObjectId(id));

            if(task == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Task not found");
            }

            _taskService.Delete(task);

            return this.RedirectToAction(x => x.Index()).Success("Task deleted");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunTask(string id)
        {
            var task = _taskService.FindOne(new ObjectId(id));

            if(task == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Task not found");
            }

            //TODO allow for long running tasks
            var result = TaskRunner.Run(task);

            return this.RedirectToAction(x => x.EditTask(id)).Success("Result: " + result);
        }
    }
}
