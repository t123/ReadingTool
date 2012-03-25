#region License
// AjaxMessagesController.cs is part of ReadingTool
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
using MongoDB.Bson;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Helpers;
using ReadingTool.Extensions;
using ReadingTool.Models.Search;
using ReadingTool.Models.View.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxMessagesController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";
        private const int LIMIT = 20;

        private readonly IUserService _userService;
        private readonly IMessageService _messageService;

        public AjaxMessagesController(
            IUserService userService,
            IMessageService messageService
            )
        {
            _userService = userService;
            _messageService = messageService;
        }

        public void Index()
        {
        }

        [HttpPost]
        public string UnreadMessageCount()
        {
            var count = _messageService.UnreadMessageCount();
            return count > 0 ? count.ToString() : "";
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonNetResult SearchIn(string[] folders, string filter, int page)
        {
            var model = new SearchModel<MessageSearchItemModel>();

            var messages = _messageService.SearchIn(folders, filter);

            var filtered = messages.Select(x =>
                                        new MessageSearchItemModel()
                                            {
                                                Id = x.MessageId.ToString(),
                                                IsStarred = x.IsStarred,
                                                IsRead = x.IsRead,
                                                Subject = x.Subject,
                                                OwnerId = x.Owner,
                                                ToId = x.To.ToArray(),
                                                FromId = x.From,
                                                ActualDate = x.Created
                                            }
                )
                .Skip((page - 1) * LIMIT)
                .Take(LIMIT)
                .ToArray();

            var users = FindUserForMessages(filtered);
            foreach(var message in filtered)
            {
                if((DateTime.Now - message.ActualDate).TotalHours < DateTime.Now.Hour)
                {
                    message.Date = Formatters.FormatTimespan(DateTime.Now - message.ActualDate.ToLocalTime()) + " ago";
                }
                else
                {
                    message.Date = message.ActualDate.ToString(SystemSettings.Instance.Values.Formats.LongDateFormat);
                }

                var fromUser = users.GetValueOrDefault(message.FromId, value => UserSimpleModel.Unknown());
                var toUser = users.GetValueOrDefault(message.OwnerId, value => UserSimpleModel.Unknown());

                message.From = new
                                    {
                                        id = fromUser.UserId,
                                        username = fromUser.Username,
                                        name = fromUser.Name
                                    };

                message.To = new[]
                                 {
                                    new    
                                    {
                                         id = toUser.UserId,
                                         username = toUser.Username,
                                         name = toUser.Name
                                     }
                                 };
            }

            model.Items = filtered;
            model.TotalItems = (int)messages.Count();
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)LIMIT);

            return new JsonNetResult() { Data = model };
        }

        private Dictionary<ObjectId, UserSimpleModel> FindUserForMessages(IEnumerable<MessageSearchItemModel> messages)
        {
            List<ObjectId> ids = new List<ObjectId>();

            var to = messages.Select(x => x.ToId);

            ids.AddRange(messages.Select(x => x.OwnerId));
            ids.AddRange(messages.Select(x => x.FromId));
            foreach(var id in to) ids.AddRange(id);
            ids = ids.Distinct().ToList();

            return _userService
                .FindAllById(ids)
                .ToDictionary(x => x.UserId, x => new UserSimpleModel { Name = x.Fullname, Username = x.Username, UserId = x.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonNetResult SearchOut(string[] folders, string filter, int page)
        {
            var model = new SearchModel<MessageSearchItemModel>();

            var messages = _messageService.SearchOut(folders, filter);

            var filtered = messages.Select(x =>
                                           new MessageSearchItemModel()
                                               {
                                                   Id = x.MessageId.ToString(),
                                                   Date = x.Created.ToString(SystemSettings.Instance.Values.Formats.ShortDateFormat),
                                                   IsStarred = x.IsStarred,
                                                   IsRead = x.IsRead,
                                                   Subject = x.Subject,
                                                   OwnerId = x.Owner,
                                                   ToId = x.To.ToArray(),
                                                   FromId = x.From
                                               }
                )
                .Skip((page - 1) * LIMIT)
                .Take(LIMIT)
                .ToArray();

            var users = FindUserForMessages(filtered);
            foreach(var message in filtered)
            {
                var fromUser = users.GetValueOrDefault(message.FromId, value => UserSimpleModel.Unknown());
                var unknown = UserSimpleModel.Unknown();

                message.From = new
                                   {
                                       id = fromUser.UserId,
                                       username = fromUser.Username,
                                       name = fromUser.Name
                                   };

                message.To = message
                    .ToId
                    .Select
                    (
                        y => users.ContainsKey(y)
                                 ? new
                                       {
                                           id = users[y].UserId.ToString(),
                                           username = users[y].Username,
                                           name = users[y].Name
                                       }
                                 : new
                                       {
                                           id = unknown.UserId.ToString(),
                                           username = unknown.Username,
                                           name = unknown.Name
                                       }
                    ).ToArray();
            }

            model.Items = filtered;
            model.TotalItems = (int)messages.Count();
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)LIMIT);

            return new JsonNetResult() { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Star(string id, bool status)
        {
            _messageService.Star(id, status);
            return Json(new
            {
                id = id,
                status = status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string id)
        {
            _messageService.Delete(id);
            return Json(id);
        }
    }
}
