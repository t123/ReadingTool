#region License
// MessagesController.cs is part of ReadingTool
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
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.Message;
using ReadingTool.Models.View.Message;
using ReadingTool.Models.View.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class MessagesController : BaseController
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessagesController(IUserService userService, IMessageService messageService)
        {
            _userService = userService;
            _messageService = messageService;
        }

        private Dictionary<ObjectId, UserSimpleModel> FindUserForMessages(Message message)
        {
            return FindUserForMessages(new List<Message>() { message });
        }

        private Dictionary<ObjectId, UserSimpleModel> FindUserForMessages(IEnumerable<Message> messages)
        {
            List<ObjectId> ids = new List<ObjectId>();

            var to = messages.Select(x => x.To);

            ids.AddRange(messages.Select(x => x.Owner));
            ids.AddRange(messages.Select(x => x.From));
            foreach(var id in to) ids.AddRange(id);
            ids = ids.Distinct().ToList();

            return _userService.FindAllById(ids).ToDictionary(x => x.UserId, x => Mapper.Map<User, UserSimpleModel>(x));
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Send(string to)
        {
            User user = _userService.FindOne(UserId);
            if(!user.ReceiveMessages)
            {
                return this.RedirectToAction(x => x.Disabled());
            }

            ViewBag.To = to ?? "";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Send(MessageModel model)
        {
            User currentUser = _userService.FindOne(UserId);
            if(!currentUser.ReceiveMessages)
            {
                return this.RedirectToAction(x => x.Disabled());
            }

            IList<ObjectId> to = new List<ObjectId>();

            if(!string.IsNullOrEmpty(model.To))
            {
                foreach(var name in model.To.Split(new[] { TagHelper.TAG_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var user = _userService.FindOneByUsername(name);
                    if(user == null)
                    {
                        ModelState.AddModelError("To", string.Format("{0} is not a valid username", name));
                    }
                    else if(!user.ReceiveMessages)
                    {
                        ModelState.AddModelError("To", string.Format("{0} has not enabled messages", name));
                    }
                    else
                    {
                        to.Add(user.UserId);
                    }
                }
            }

            if(to.Count == 0)
            {
                ModelState.AddModelError("To", "Please enter some valid usernames");
            }

            if(ModelState.IsValid)
            {
                var md = MarkdownHelper.Default();

                _messageService.Send(to, model.Subject, md.Transform(model.Body));

                return this.RedirectToAction(x => x.Index()).Success("Message sent");
            }

            return View(model).Error(Common.Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(Message), typeof(MessageViewModel))]
        public ActionResult Read(string id)
        {
            var message = _messageService.FindOne(id);

            if(message == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Message not found");
            }

            ViewBag.Users = FindUserForMessages(message);

            return View(message);
        }

        public ActionResult Disabled()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Star(string id)
        {
            _messageService.Star(id, true);
            return this.RedirectToAction(x => x.Read(id)).Success("Message starred");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unstar(string id)
        {
            _messageService.Star(id, false);
            return this.RedirectToAction(x => x.Read(id)).Success("Message unstarred");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            _messageService.Delete(id);
            return this.RedirectToAction(x => x.Index()).Success("Message deleted");
        }
    }
}
