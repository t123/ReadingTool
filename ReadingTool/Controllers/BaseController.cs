#region License
// BaseController.cs is part of ReadingTool
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

using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Common.Keys;
using ReadingTool.Entities.Identity;

namespace ReadingTool.Controllers
{
    [HandleError]
    public class BaseController : Controller
    {
        protected ObjectId UserId
        {
            get
            {
                return HttpContext.User.Identity.IsAuthenticated == false ? ObjectId.Empty : (HttpContext.User.Identity as UserIdentity).UserId;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewData[ViewDataKeys.CURRENT_MENU] = filterContext.RouteData.Values["controller"] ?? "";
            base.OnActionExecuting(filterContext);
        }
    }
}
