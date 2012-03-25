#region License
// Global.asax.cs is part of ReadingTool
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
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using LowercaseRoutesMVC;
using Newtonsoft.Json;
using ReadingTool.Binders;
using ReadingTool.Common;
using ReadingTool.Common.Keys;
using ReadingTool.DependencyResolution;
using ReadingTool.Entities.Identity;
using ReadingTool.Filters;
using ReadingTool.Services;
using ReadingTool.ViewEngine;

namespace ReadingTool
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new UrlFilter() { Order = 1 });
            filters.Add(new ElmahHandledErrorLoggerFilter());
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            #region routes
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("content/{*path}");
            routes.IgnoreRoute("images/{*path}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRouteLowercase(
               "Homepage",
               "",
               new { controller = "home", action = "index" },
               new[] { "ReadingTool.Controllers" }
            );

            routes.MapRouteLowercase(
               "About",
               "about",
               new { controller = "home", action = "about" },
               new[] { "ReadingTool.Controllers" }
            );

            routes.MapRouteLowercase(
               "Contact",
               "contact",
               new { controller = "home", action = "contact" },
               new[] { "ReadingTool.Controllers" }
            );

            routes.MapRouteLowercase(
               "Legalese",
               "legalese",
               new { controller = "home", action = "legalese" },
               new[] { "ReadingTool.Controllers" }
            );

            routes.MapRouteLowercase(
               "Denied",
               "denied",
               new { controller = "registration", action = "denied" },
               new[] { "ReadingTool.Controllers" }
            );

            routes.MapRoute(
                "PublicProfile",
                "profiles/{username}",
                new { controller = "profiles", action = "index" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRoute(
                "SendMessage",
                "messages/send/{to}",
                new { controller = "messages", action = "send" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRoute(
                "GroupRead",
                "groups/read/{id}/{groupId}",
                new { controller = "groups", action = "read" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRoute(
                "GroupReadParallel",
                "groups/readparallel/{id}/{groupId}",
                new { controller = "groups", action = "readparallel" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRoute(
                "GroupWatch",
                "groups/watch/{id}/{groupId}",
                new { controller = "groups", action = "watch" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRoute(
                "SharedWords",
                "words/shared/{word}/{languageId}",
                new { controller = "words", action = "shared" },
                new[] { "ReadingTool.Controllers" }
                );

            routes.MapRouteLowercase(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "home", action = "index", id = UrlParameter.Optional }, // Parameter defaults
                new[] { "ReadingTool.Controllers" }
            );
            #endregion
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(Server.MapPath("~/bin/log4net.config")));

            Logger.Info("Application Started");

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new LowereRazorViewEngine());
            StructuremapMvc.Start();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Mappings.RegisterMappings();

            ModelBinders.Binders.DefaultBinder = new MongoIdBinder();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if(authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var cookieData = JsonConvert.DeserializeObject<UserCookieModel>(authTicket.UserData);
                    IIdentity identity = HttpContext.Current.User.Identity;
                    UserPrincipal newUser = SecurityManager.ConstructUserPrincipal(identity, cookieData);
                    Context.User = newUser;
                    System.Threading.Thread.CurrentPrincipal = newUser;
                }
                catch
                {
                    FormsAuthentication.SignOut();
                }
            }
        }

        void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            InitialiseForIis7.Initialise(context);
        }
    }
}