#region License
// ParsersController.cs is part of ReadingTool
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

using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Areas.Admin.Models;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class ParsersController : BaseController
    {
        private readonly ITextParsers _textParsers;

        public ParsersController(ITextParsers textParsers)
        {
            _textParsers = textParsers;
        }

        [AutoMap(typeof(IEnumerable<TextParser>), typeof(IEnumerable<TextParserModel>))]
        public ActionResult Index()
        {
            return View(_textParsers.FindAll());
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(TextParserModel model)
        {

            if(ModelState.IsValid)
            {
                var parser = Mapper.Map<TextParserModel, TextParser>(model);
                _textParsers.Save(parser);

                return this.RedirectToAction(x => x.Index()).Success("Parser added");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(TextParser), typeof(TextParserModel))]
        public ActionResult Edit(string id)
        {
            var parser = _textParsers.FindOne(id);

            if(parser == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Parser not found");
            }

            return View(parser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, TextParserModel model)
        {

            if(ModelState.IsValid)
            {
                var parser = _textParsers.FindOne(id);
                parser.FullPath = model.FullPath;
                parser.Arguments = model.Arguments;
                parser.Name = model.Name;
                _textParsers.Save(parser);

                return this.RedirectToAction(x => x.Index()).Success("Parser updated");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var parser = _textParsers.FindOne(new ObjectId(id));

            if(parser == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Parser not found");
            }

            _textParsers.Delete(parser);

            return this.RedirectToAction(x => x.Index()).Success("Parser deleted");
        }
    }
}
