using System;
using System.Web.Mvc;
using System.Web.Routing;
using ReadingTool.Common;

namespace ReadingTool.Site.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Admin");

        public CustomAuthorizeAttribute()
        {
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string clientIP = "UNKNOWN IP";
            try
            {
                clientIP = filterContext.RequestContext.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if(!string.IsNullOrEmpty(clientIP))
                {
                    string[] forwardedIps = clientIP.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    clientIP = forwardedIps[forwardedIps.Length - 1];
                }
                else
                {
                    clientIP = filterContext.RequestContext.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }

            try
            {
                if(filterContext.RequestContext.HttpContext.Request.IsLocal)
                {
                    var user = filterContext.HttpContext.User.Identity as UserIdentity;

                    if(user != null)
                    {
                        Logger.WarnFormat("{2}: Local request for user {0} ({1}), access granted", user.Data.Username, user.UserId, clientIP);
                        return;
                    }
                }


                if(filterContext.HttpContext != null)
                {
                    if(filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
                    {
                        if(!string.IsNullOrEmpty(Roles))
                        {
                            string[] requestedRoles = Roles.Split(',');
                            bool isInRole = false;
                            foreach(var role in requestedRoles)
                            {
                                if(filterContext.HttpContext.User.IsInRole(role))
                                {
                                    isInRole = true;
                                    break;
                                }
                            }

                            if(!isInRole)
                            {
                                var user = filterContext.HttpContext.User.Identity as UserIdentity;
                                if(user != null)
                                {
                                    Logger.WarnFormat("{3}: Denied, user {0} ({1}) not in roles: {2}", user.Data.Username, user.UserId, Roles, clientIP);
                                }
                                else
                                {
                                    Logger.WarnFormat("{3}: Denied, user {0} ({1}) not in roles: {2}", "UNKNOWN", filterContext.HttpContext.User.Identity.Name, Roles, clientIP);
                                }

                                filterContext.Result = new RedirectToRouteResult(
                                    new RouteValueDictionary
                                        {
                                            {"controller", "~~Home"},
                                            {"action", "Index"},
                                            {"area", ""}
                                        });

                                return;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }

            base.OnAuthorization(filterContext);
        }
    }
}