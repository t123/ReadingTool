#region License
// AuthorizeAttribute.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

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