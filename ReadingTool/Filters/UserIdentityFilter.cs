#region License
// UserIdentityFilter.cs is part of ReadingTool
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
using ReadingTool.Entities.Identity;
using ReadingTool.Models.View.User;

namespace ReadingTool.Filters
{
    public class UserIdentityFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            UserAuthenticationViewModel user = null;
            var identity = context.HttpContext.User.Identity as UserIdentity;

            if(identity != null && context.HttpContext.User.Identity.IsAuthenticated)
            {
                user = new UserAuthenticationViewModel()
                {
                    IsAuthenticated = true,
                    Name = identity.Name,
                    Roles = identity.Roles,
                    UserId = identity.UserId,
                    DisplayName = identity.DisplayName
                };
            }

            context.Controller.ViewBag.UserIdentity = user ?? new UserAuthenticationViewModel();

            base.OnActionExecuting(context);
        }

        //public override void OnResultExecuting(ResultExecutingContext context)
        //{
        //    base.OnResultExecuting(context);
        //    UserAuthenticationViewModel user = null;
        //    var identity = context.HttpContext.User.Identity as UserIdentity;

        //    if(identity != null && context.HttpContext.User.Identity.IsAuthenticated)
        //    {
        //        user = new UserAuthenticationViewModel()
        //                   {
        //                       IsAuthenticated = true,
        //                       Name = identity.Name,
        //                       Roles = identity.Roles,
        //                       UserId = identity.UserId,
        //                       DisplayName = identity.DisplayName
        //                   };
        //    }

        //    context.Controller.ViewBag.UserIdentity = user ?? new UserAuthenticationViewModel();
        //}
    }
}