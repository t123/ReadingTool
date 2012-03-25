#region License
// SystemSettingsController.cs is part of ReadingTool
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
using System.Text;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MvcContrib;
using Newtonsoft.Json;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Keys;
using ReadingTool.Extensions;
using ReadingTool.Models.Create.Import;
using ReadingTool.Services;
using JsonReader = Newtonsoft.Json.JsonReader;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class SystemSettingsController : BaseController
    {
        private readonly ISystemSettingsService _settingsService;

        private object FormatSystemSettingValues(SystemSystemValues settings)
        {
            return (object)settings.ToJson(new JsonWriterSettings { Indent = true }).Trim();
        }

        public SystemSettingsController(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public ActionResult Index()
        {
            var settings = SystemSettings.Instance.Values;
            return View(FormatSystemSettingValues(settings));
        }

        public ActionResult DbSettings()
        {
            var settings = _settingsService.Settings(ConfigurationManager.AppSettings[CacheKeys.SETTINGS_KEY]);
            return View(FormatSystemSettingValues(settings));
        }

        public ActionResult Reload()
        {
            SystemSettings.Instance.Reload();

            return this.RedirectToAction(x => x.Index()).Success("Settings reloaded from db");
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var settings = _settingsService.Settings(ConfigurationManager.AppSettings[CacheKeys.SETTINGS_KEY]);
            return View(FormatSystemSettingValues(settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(string settings)
        {
            string message = "Settings not saved";

            try
            {
                if(ModelState.IsValid)
                {
                    var values = JsonConvert.DeserializeObject<SystemSystemValues>(settings);
                    SystemSettings.Instance.Import(values);

                    return this.RedirectToAction(x => x.Edit()).Success("Settings saved");
                }
            }
            catch(Exception e)
            {
                message = "There is an error in your JSON: " + e.Message;
            }

            return View((object)settings).Error(message);
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(ImportDataUploadModel model)
        {
            if(ModelState.IsValid)
            {
                using(var tr = new StreamReader(model.File.InputStream))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    using(JsonReader reader = new JsonTextReader(tr))
                    {
                        var data = serializer.Deserialize<SystemSystemValues>(reader);
                        SystemSettings.Instance.Import(data);
                    }
                }

                return this.RedirectToAction(x => x.Index()).Success("Settings imported and reloaded");
            }

            return View(model).Error("Please correct the errors below");
        }

        [HttpGet]
        public FileResult Export()
        {
            string json = JsonConvert.SerializeObject(SystemSettings.Instance.Values, Formatting.Indented);
            return File(Encoding.UTF8.GetBytes(json), "text/plain", "settings.json");
        }
    }
}
