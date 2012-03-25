#region License
// UsersController.cs is part of ReadingTool
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
using System.Web.Mvc;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.User;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AutoMap(typeof(IEnumerable<User>), typeof(IEnumerable<ProfileModel>))]
        public ActionResult Index(int page)
        {
            ViewBag.Page = page;

            var search = _userService.FindAll(page);
            ViewBag.Total = search.Item1;

            return View(search.Item2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            ObjectId userId;

            if(!ObjectId.TryParse(id, out userId))
            {
                return this.RedirectToAction(x => x.Index(1)).Error("User not deleted; invalid id");
            }

            _userService.DeleteData(userId, true);

            return this.RedirectToAction(x => x.Index(1)).Error("User deleted");
        }
    }
}
