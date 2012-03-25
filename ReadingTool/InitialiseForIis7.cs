#region License
// InitialiseForIis7.cs is part of ReadingTool
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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using MongoDB.Bson;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Keys;
using ReadingTool.Entities;
using ReadingTool.Models.View.Url;
using ReadingTool.Services;

namespace ReadingTool
{
    public class InitialiseForIis7
    {
        private static bool _isInitialised { get; set; }
        private static object _lock = new object();

        public static void Initialise(HttpContext context)
        {
            if(_isInitialised) return;

            lock(_lock)
            {
                if(_isInitialised) return;

                InitRoutes(context);
                InitSettings(context);
                InitXsl(context);

                _isInitialised = true;
            }
        }

        private static void InitXsl(HttpContext context)
        {
            var xslService = DependencyResolver.Current.GetService<IXslService>();

            if(!xslService.FindAll().Any())
            {
                using(TextReader tr = new StreamReader(Path.Combine(SystemSettings.Instance.Values.Site.BasePath, @"App_Data/XSL/readparallel.xsl"), Encoding.UTF8))
                {
                    xslService.Save(new Xsl()
                    {
                        Name = "Parallel",
                        SystemLanguageId = ObjectId.Empty,
                        ItemType = ItemType.Text,
                        XslTransform = tr.ReadToEnd()
                    });
                }


                using(TextReader tr = new StreamReader(Path.Combine(SystemSettings.Instance.Values.Site.BasePath, @"App_Data/XSL/read.xsl"), Encoding.UTF8))
                {
                    xslService.Save(new Xsl()
                    {
                        Name = "Single",
                        SystemLanguageId = ObjectId.Empty,
                        ItemType = ItemType.Text,
                        XslTransform = tr.ReadToEnd()
                    });
                }

                using(TextReader tr = new StreamReader(Path.Combine(SystemSettings.Instance.Values.Site.BasePath, @"App_Data/XSL/watch.xsl"), Encoding.UTF8))
                {
                    xslService.Save(new Xsl()
                    {
                        Name = "Watch",
                        SystemLanguageId = ObjectId.Empty,
                        ItemType = ItemType.Video,
                        XslTransform = tr.ReadToEnd()
                    });
                }
            }
        }

        private static void InitSettings(HttpContext context)
        {
            var systemSettingService = DependencyResolver.Current.GetService<ISystemSettingsService>();
            bool changed = false;
            if(string.IsNullOrEmpty(SystemSettings.Instance.Values.Site.Domain))
            {
                if(string.IsNullOrEmpty(ConfigurationManager.AppSettings["DomainUrl"]))
                {
                    SystemSettings.Instance.Values.Site.Domain = context.Request.Url.GetLeftPart(UriPartial.Authority);
                }
                else
                {
                    SystemSettings.Instance.Values.Site.Domain = ConfigurationManager.AppSettings["DomainUrl"];
                }

                changed = true;
            }

            if(string.IsNullOrEmpty(SystemSettings.Instance.Values.Site.BasePath))
            {
                SystemSettings.Instance.Values.Site.BasePath = context.Server.MapPath("/");
                changed = true;
            }

            if(changed)
            {
                systemSettingService.Save(SystemSettings.Instance.Values);
            }
        }

        private static void InitRoutes(HttpContext context)
        {
            if(HttpRuntime.Cache[CacheKeys.URLS] == null)
            {
                UrlModel urlModel = new UrlModel()
                {
                    Texts = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Texts" }, { "action", "Index" } }).VirtualPath,
                    Videos = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Texts" }, { "action", "Index" } }).VirtualPath,
                    Words = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Words" }, { "action", "Index" } }).VirtualPath,
                    Languages = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Languages" }, { "action", "Index" } }).VirtualPath,
                    MyAccount = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "MyAccount" }, { "action", "Index" } }).VirtualPath,
                    Messages = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Messages" }, { "action", "Index" } }).VirtualPath,
                    Groups = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Groups" }, { "action", "Index" } }).VirtualPath,
                    Profiles = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Profiles" }, { "action", "Index" } }).VirtualPath,

                    Ajax = new UrlModel.AjaxUrls()
                    {
                        Base = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "Ajax" }, { "action", "Index" } }).VirtualPath,
                        Items = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "AjaxItems" }, { "action", "Index" } }).VirtualPath,
                        Groups = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "AjaxGroups" }, { "action", "Index" } }).VirtualPath,
                        Words = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "AjaxWords" }, { "action", "Index" } }).VirtualPath,
                        Messages = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "AjaxMessages" }, { "action", "Index" } }).VirtualPath,
                        Languages = RouteTable.Routes.GetVirtualPathForArea(context.Request.RequestContext, new RouteValueDictionary() { { "controller", "AjaxLanguages" }, { "action", "Index" } }).VirtualPath,
                    }
                };

                HttpRuntime.Cache.Insert(CacheKeys.URLS, urlModel, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            }
        }
    }
}