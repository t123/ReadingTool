using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Entities;

namespace ReadingTool.Site.Helpers
{
    public class UserHelper
    {
        public const long NOT_LOGGED_IN_ID = -1;

        public static long CurrentUserId(HttpContext context)
        {
            if(
                context == null ||
                context.User == null ||
                context.User.Identity == null ||
                !context.User.Identity.IsAuthenticated
                ) return NOT_LOGGED_IN_ID;

            IUserIdentity identity = HttpContext.Current.User.Identity as IUserIdentity;

            if(identity == null) return NOT_LOGGED_IN_ID;

            return identity.UserId;
        }

        public static long CurrentUserId(HttpContextBase context)
        {
            if(
                context == null ||
                context.User == null ||
                context.User.Identity == null ||
                !context.User.Identity.IsAuthenticated
                ) return NOT_LOGGED_IN_ID;

            IUserIdentity identity = HttpContext.Current.User.Identity as IUserIdentity;

            if(identity == null) return NOT_LOGGED_IN_ID;

            return identity.UserId;
        }
    }

    public static class HttpContextHelper
    {
        public static long CurrentUserId(this System.Web.HttpContext context)
        {
            return UserHelper.CurrentUserId(context);
        }
    }

    public static class HttpContextBaseHelper
    {
        public static long CurrentUserId(this System.Web.HttpContextBase context)
        {
            return UserHelper.CurrentUserId(context);
        }
    }

    public static class ControllerHelper
    {
        public static long CurrentUserId(this Controller controller)
        {
            if(controller == null) return UserHelper.NOT_LOGGED_IN_ID;
            return UserHelper.CurrentUserId(controller.HttpContext);
        }
    }
}