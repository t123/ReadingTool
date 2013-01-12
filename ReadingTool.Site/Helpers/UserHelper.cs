using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Entities;

namespace ReadingTool.Site.Helpers
{
    public class UserHelper
    {
        public static readonly ObjectId NOT_LOGGED_IN_ID = ObjectId.Empty;

        public static ObjectId CurrentUserId(HttpContext context)
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

        public static ObjectId CurrentUserId(HttpContextBase context)
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
        public static ObjectId CurrentUserId(this System.Web.HttpContext context)
        {
            return UserHelper.CurrentUserId(context);
        }
    }

    public static class HttpContextBaseHelper
    {
        public static ObjectId CurrentUserId(this System.Web.HttpContextBase context)
        {
            return UserHelper.CurrentUserId(context);
        }
    }

    public static class ControllerHelper
    {
        public static ObjectId CurrentUserId(this Controller controller)
        {
            if(controller == null) return UserHelper.NOT_LOGGED_IN_ID;
            return UserHelper.CurrentUserId(controller.HttpContext);
        }
    }
}