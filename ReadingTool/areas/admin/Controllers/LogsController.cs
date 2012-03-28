#region License
// LogsController.cs is part of ReadingTool
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
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Extensions;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class LogsController : BaseController
    {
        private readonly ILogService _logService;
        private readonly IItemService _itemService;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LogsController(ILogService logService, IItemService itemService)
        {
            _logService = logService;
            _itemService = itemService;
        }

        public ActionResult Index(int page)
        {
            ViewBag.Page = page;
            return View(_logService.FindAll(page));
        }

        public ActionResult DeleteAll()
        {
            _logService.DeleteAll();
            return this.RedirectToAction(x => x.Index(1)).Success("Logs deleted");
        }

        public ActionResult Log4NetDetails()
        {
            Logger.Info("Called Log4NetDetails");
            return View(Logger);
        }

        public ActionResult ParsingTimes(int page)
        {
            ViewBag.Page = page;
            return View(_itemService.FindAllParsingTimes(page));
        }

        public ActionResult DeleteAllParsingTimes()
        {
            _itemService.DeleteAllParsingTimes();
            return this.RedirectToAction(x => x.ParsingTimes(1));
        }
    }
}
