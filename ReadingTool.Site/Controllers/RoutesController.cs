#region License
// RoutesController.cs is part of ReadingTool.Site
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using ReadingTool.Site.Attributes;
using ServiceStack.Text;

namespace ReadingTool.Site.Controllers.Home
{
    public class RoutesController : Controller
    {
        public ActionResult Index()
        {
            StringBuilder js = new StringBuilder();
            string queryString = Request.QueryString["c"] ?? "";
            string globalName = Request.QueryString["name"] ?? "routes";

            if(string.IsNullOrWhiteSpace(queryString))
            {
                goto final;
            }

            var controllers = queryString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            js.AppendFormat("{0}={{", globalName);

            foreach(var c in controllers)
            {
                int indexOfPeriod = c.LastIndexOf('.');

                string actualControllerName = indexOfPeriod < 0
                                                  ? c + "Controller"
                                                  : c.Substring(indexOfPeriod + 1) + "Controller";
                var controller = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(x =>
                                    x.Name.Equals(actualControllerName, StringComparison.InvariantCultureIgnoreCase) &&
                                    x.BaseType == typeof(Controller)
                    );

                if(controller == null)
                {
                    continue;
                }

                var routeNames = controller.GetMethods().Where(x => x.GetCustomAttribute<AjaxRouteAttribute>() != null).ToArray();

                if(routeNames.Any())
                {
                    js.AppendFormat("\t{0}: {{\n", controller.Name.Replace("Controller", "").ToCamelCase());

                    foreach(var r in routeNames)
                    {
                        var ara = r.GetCustomAttribute<AjaxRouteAttribute>();
                        string name = string.IsNullOrEmpty(ara.Name) ? r.Name : ara.Name;
                        js.AppendFormat("\t\t{0}: '{1}',\n", name.ToCamelCase(), Url.Action(r.Name, c, new { }));
                    }

                    js.AppendLine("\t},");
                }
            }

            js.AppendLine("};");

        final:
#if DEBUG
            js.AppendFormat("console.log({0});", globalName);
#else
            js.Replace(" ", "").Replace("\t", "").Replace("\n", "");
#endif
            js.Insert(0, "var ");
            return new JavaScriptResult() { Script = js.ToString() };
        }
    }
}
