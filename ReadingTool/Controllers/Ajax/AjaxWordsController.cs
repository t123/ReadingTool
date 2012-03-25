#region License
// AjaxWordsController.cs is part of ReadingTool
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Models.Search;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxWordsController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";
        private const int LIMIT = 20;

        private readonly IWordService _wordService;
        private readonly ILanguageService _languageService;

        public AjaxWordsController(
            IWordService wordService,
            ILanguageService languageService
            )
        {
            _wordService = wordService;
            _languageService = languageService;
        }

        public void Index()
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonNetResult Search(
            string filter,
            string[] boxes,
            string[] languages,
            string[] states,
            string orderBy,
            string orderDirection,
            int limit,
            int page
            )
        {
            var model = new SearchModel<WordSearchItemModel>();
            var result = _wordService.SearchWords(filter, languages, states, boxes, orderBy, orderDirection, limit, page);

            var items = result.Items.Select(x =>
                                            new WordSearchItemModel()
                                                {
                                                    LanguageName = x.LanguageName,
                                                    LanguageColour = x.LanguageColour,
                                                    Id = x.WordId.ToString(),
                                                    State = EnumHelper.GetDescription(x.State),
                                                    Box = x.Box,
                                                    Definition = x.FullDefinition,
                                                    Word = x.WordPhrase,
                                                    Sentence = x.Sentence
                                                }
                ).ToArray();

            foreach(var item in items)
            {
                string sentence = Server.HtmlEncode(item.Sentence);
                string encodedWord = Server.HtmlEncode(item.Word);

                if(!string.IsNullOrEmpty(item.Definition))
                {
                    item.Sentence = sentence.Replace(encodedWord, string.Format(@"<a class=""s"" title=""{0}"">{1}</a>", Server.HtmlEncode(item.Definition), encodedWord));
                }
                else
                {
                    item.Sentence = sentence.Replace(encodedWord, string.Format(@"<strong><u>{0}</u></strong>", encodedWord));
                }

                item.Definition = "";
            }

            model.Items = items;
            model.TotalItems = result.Count;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)limit);

            return new JsonNetResult() { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult AddTags(string[] items, string tagsToAdd)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(OK);
                if(string.IsNullOrEmpty(tagsToAdd)) return Json(OK);

                string[] tagList = TagHelper.Split(tagsToAdd);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var word = _wordService.FindOne(tid);
                    if(word == null) continue;
                    word.Tags = TagHelper.Merge(word.Tags, tagList);
                    _wordService.Save(word);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult RemoveTags(string[] items, string tagsToRemove)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(OK);
                if(string.IsNullOrEmpty(tagsToRemove)) return Json(OK);

                string[] tagList = TagHelper.Split(tagsToRemove);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var word = _wordService.FindOne(tid);
                    if(word == null) continue;
                    word.Tags = TagHelper.Remove(word.Tags, tagList);
                    _wordService.Save(word);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] items)
        {
            try
            {
                if(items == null || items.Length == 0)
                    return Json(OK);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    _wordService.Delete(tid);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        private static WordState GetEnumFromDescription(string name)
        {
            FieldInfo[] fi = typeof(WordState).GetFields();
            foreach(var field in fi)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if(attributes.Length > 0 && attributes[0].Description.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (WordState)field.GetValue(null);
                }
            }

            throw new Exception("Description not found");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeStatus(string[] items, string newState)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(new { result = OK });

                WordState state = GetEnumFromDescription(newState);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var word = _wordService.FindOne(tid);
                    if(word == null) continue;
                    word.State = state;
                    _wordService.Save(word);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }
    }
}
