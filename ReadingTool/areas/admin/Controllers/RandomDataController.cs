#region License
// RandomDataController.cs is part of ReadingTool
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

using System.Linq;
using System.Web.Mvc;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Common.Enums;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class RandomDataController : BaseController
    {
        private readonly ILanguageService _languageService;
        private readonly IItemService _itemService;

        public RandomDataController
            (
            ILanguageService languageService,
            IItemService itemService
            )
        {
            _languageService = languageService;
            _itemService = itemService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string id)
        {
#if DEBUG
            if(string.IsNullOrEmpty(id))
            {
                return this.RedirectToAction(x => x.Index()).Error("No random data created");
            }

            var languages = _languageService.FindAllForUser(new ObjectId("4f68da3e35955a0eecbb9f40")).ToArray();

            foreach(var language in languages)
            {
                for(int i = 0; i < 12; i++)
                {
                    var video = new Item()
                                    {
                                        ItemId = ObjectId.GenerateNewId(),
                                        CollectionName = "collection" + i % 20,
                                        CollectionNo = i % 20,
                                        Title = " Lorem Ipsum " + i,
                                        LanguageId = language.LanguageId,
                                        Tags = i % 15 == 0 ? new[] { "tag1", "tag2", "tag3" } : new[] { "" },
                                        L1Text = string.Empty,
                                        L2Text = string.Empty,
                                        LanguageName = language.Name,
                                        LanguageColour = language.Colour,
                                        ItemType = i % 2 == 0 ? ItemType.Video : ItemType.Text,
                                        Owner = new ObjectId("4f68da3e35955a0eecbb9f40"),
                                        SystemLanguageId = language.SystemLanguageId
                                    };

                    _itemService.Save(video);
                }
            }

            return this.RedirectToAction(x => x.Index()).Success("Random data created");
#else
            return this.RedirectToAction(x => x.Index()).Error("Debug not enabled");
#endif
        }
    }
}
