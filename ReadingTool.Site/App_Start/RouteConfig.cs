using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcCodeRouting;

namespace ReadingTool.Site
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapCodeRoutes(
                rootController: typeof(Controllers.HomeController),
                settings: new CodeRoutingSettings
                    {
                        UseImplicitIdToken = true,
                        RouteFormatter = args => Regex.Replace(args.OriginalSegment, @"([a-z])([A-Z])", "$1-$2").ToLowerInvariant()
                    }
                );

            //routes.MapCodeRoutes(
            //    baseRoute: "account",
            //    rootController: typeof(Controllers.AccountController),
            //    settings: new CodeRoutingSettings
            //    {
            //        UseImplicitIdToken = true,
            //        RouteFormatter = args => Regex.Replace(args.OriginalSegment, @"([a-z])([A-Z])", "$1-$2").ToLowerInvariant()
            //    }
            //    );

            //routes.MapCodeRoutes(
            //    baseRoute: "user",
            //    rootController: typeof(Controllers.User.MyAccountController),
            //    settings: new CodeRoutingSettings
            //    {
            //        UseImplicitIdToken = true,
            //        RouteFormatter = args => Regex.Replace(args.OriginalSegment, @"([a-z])([A-Z])", "$1-$2").ToLowerInvariant()
            //    }
            //    );



            ViewEngines.Engines.EnableCodeRouting();
        }
    }
}