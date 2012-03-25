#region License
// LowereRazorViewEngine.cs is part of ReadingTool
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

using System.Web.Mvc;

namespace ReadingTool.ViewEngine
{
    /// <summary>
    /// Standard RazorViewEngine, remove the VB references and lowercased the paths in the createview methods for Mono.
    /// </summary>
    public class LowereRazorViewEngine : BuildManagerViewEngine
    {
        internal static readonly string ViewStartFileName = "_ViewStart";

        public LowereRazorViewEngine()
            : this(null)
        {
        }

        public LowereRazorViewEngine(IViewPageActivator viewPageActivator)
            : base(viewPageActivator)
        {
            AreaViewLocationFormats = new[] {
                "~/areas/{2}/views/{1}/{0}.cshtml",
                "~/areas/{2}/views/shared/{0}.cshtml",
            };
            AreaMasterLocationFormats = new[] {
                "~/areas/{2}/views/{1}/{0}.cshtml",
                "~/areas/{2}/views/shared/{0}.cshtml",
            };
            AreaPartialViewLocationFormats = new[] {
                "~/areas/{2}/views/{1}/{0}.cshtml",
                "~/areas/{2}/views/shared/{0}.cshtml",
            };

            ViewLocationFormats = new[] {
                "~/views/{1}/{0}.cshtml",
                "~/views/shared/{0}.cshtml",
            };
            MasterLocationFormats = new[] {
                "~/views/{1}/{0}.cshtml",
                "~/views/shared/{0}.cshtml",
            };
            PartialViewLocationFormats = new[] {
                "~/views/{1}/{0}.cshtml",
                "~/views/shared/{0}.cshtml",
            };

            FileExtensions = new[] {
                "cshtml"
            };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            partialPath = partialPath.ToLowerInvariant();
            return new RazorView(controllerContext, partialPath,
                                 layoutPath: null, runViewStartPages: false, viewStartFileExtensions: FileExtensions, viewPageActivator: ViewPageActivator);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            viewPath = viewPath.ToLowerInvariant();
            masterPath = masterPath.ToLowerInvariant();

            var view = new RazorView(controllerContext, viewPath,
                                     layoutPath: masterPath, runViewStartPages: true, viewStartFileExtensions: FileExtensions, viewPageActivator: ViewPageActivator);
            return view;
        }

        /// <summary>
        /// https://gist.github.com/870574
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return base.FileExists(controllerContext, virtualPath.Replace("~", ""));
        }
    }
}
