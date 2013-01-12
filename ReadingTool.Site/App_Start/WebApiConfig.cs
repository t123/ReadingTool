using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using NinjectAdapter;
//using ReadingTool.Site.Controllers.Api;

namespace ReadingTool.Site
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Add(new JsonMediaTypeFormatter() { Indent = true, SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() } });

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //GlobalConfiguration.Configuration.MessageHandlers.Add(new AuthorizationHeaderHandler());
            config.Filters.Add(new AuthorizeAttribute());
        }
    }
}
