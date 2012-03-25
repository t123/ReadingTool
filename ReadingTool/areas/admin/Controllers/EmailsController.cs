#region License
// EmailsController.cs is part of ReadingTool
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
using System.Collections.Generic;
using System.Web.Mvc;
using MvcContrib;
using ReadingTool.Areas.Admin.Models;
using ReadingTool.Attributes;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class EmailsController : BaseController
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [AutoMap(typeof(IEnumerable<EmailTemplate>), typeof(IEnumerable<EmailTemplateModel>))]
        public ActionResult Index()
        {
            ViewBag.Queued = _emailService.QueuedCount();

            return View(_emailService.FindAll());
        }

        [HttpGet]
        public ActionResult AddMissingTemplates()
        {
            int added = 0;
            Type type = typeof(EmailTemplate.TemplateNames);
            foreach(var p in type.GetFields())
            {
                var v = (string) p.GetValue(null);
                var template = _emailService.FindOne(v);
                if(template == null)
                {
                    template = new EmailTemplate { Name = v };
                    _emailService.Save(template);
                    added++;
                }
            }

            return this.RedirectToAction(x => x.Index()).Success(string.Format("{0} templates added", added));
        }

        [HttpGet]
        [AutoMap(typeof(EmailTemplate), typeof(EmailTemplateModel))]
        public ActionResult Edit(string id)
        {
            var template = _emailService.FindOne(id);

            if(template == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Template not found");
            }

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(string id, EmailTemplateModel model)
        {
            if(ModelState.IsValid)
            {
                var template = _emailService.FindOne(id);
                template.Subject = model.Subject;
                template.Body = model.Body;
                _emailService.Save(template);

                return this.RedirectToAction(x => x.Edit(id)).Success("Template updated");
            }

            return View(model).Error("Template not updated");
        }
    }
}
