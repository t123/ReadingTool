#region License
// BaseController.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using MongoDB.Bson;
using MongoDB.Driver;
using ReadingTool.Entities;
using ReadingTool.Services;
using StructureMap;

namespace ReadingTool.API.Areas.V1.Controllers
{
    [HandleError]
    public class BaseController : Controller
    {
        protected ObjectId _userId = ObjectId.Empty;
        protected int _page;
        protected int _maxItemsPerPage = 250;
        protected Token _token = null;
        protected readonly MongoDatabase _db = MongoServer.Create(ConfigurationManager.ConnectionStrings["default"].ConnectionString).GetDatabase(ConfigurationManager.AppSettings["DBName"]);

        public BaseController()
        {
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controller = ((string)filterContext.RouteData.Values["controller"]).ToLowerInvariant();
            string action = ((string)filterContext.RouteData.Values["action"]).ToLowerInvariant();

#if !DEBUG
            if(
                !controller.Equals("user") &&
                !action.Equals("postonly") &&
                !filterContext.HttpContext.Request.HttpMethod.Equals("POST")
                )
            {
                filterContext.Result = new RedirectToRouteResult(
                                new RouteValueDictionary
                                {
                                    {"controller", "user"},
                                    {"action", "postonly"},
                                    {"area", "v1"}
                                });
            }
#endif
            if(!controller.Equals("user"))
            {
                //bool noGzip = false;
                //string[] encoding = ((string)(filterContext.HttpContext.Request.Headers["Accept-Encoding"] ?? ""))
                //    .ToLowerInvariant()
                //    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                //if(encoding == null || encoding.Length == 0)
                //{
                //    noGzip = true;
                //}
                //else if(Array.IndexOf(encoding, "gzip") < 0 && Array.IndexOf(encoding, "deflate") < 0)
                //{
                //    noGzip = true;
                //}

                //if(noGzip)
                //{
                //    filterContext.Result = new RedirectToRouteResult(
                //                new RouteValueDictionary
                //                {
                //                    {"controller", "user"},
                //                    {"action", "nocompression"},
                //                    {"area", "v1"}
                //                });

                //    return;
                //}

                if(_token == null)
                {
                    var tokenService = ObjectFactory.GetInstance<ITokenService>();
                    string tokenId = filterContext.HttpContext.Request.QueryString["t"] ?? "";
                    var token = tokenService.Find(tokenId);

                    if(token == null)
                    {
                        filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary
                                {
                                    {"controller", "user"},
                                    {"action", "authenticationfailed"},
                                    {"area", "v1"}
                                });

                        return;
                    }

                    if(!token.IsValid)
                    {
                        filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary
                                {
                                    {"controller", "user"},
                                    {"action", "limitexceeded"},
                                    {"area", "v1"}
                                });

                        return;
                    }

                    _token = token;
                    _userId = token.UserId;
                }
            }

            int page;
            if(int.TryParse(filterContext.HttpContext.Request.QueryString["page"], out page))
            {
                _page = page;
            }
            else
            {
                _page = 1;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
