#region License
// TextsController.cs is part of ReadingTool
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AutoMapper;
using Ionic.Zip;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MvcContrib;
using Newtonsoft.Json;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Exceptions;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.JSON;
using ReadingTool.Entities.Parser;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.Text;
using ReadingTool.Models.Create.Video;
using ReadingTool.Models.View.Language;
using ReadingTool.Services;
using StructureMap;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class TextsController : BaseController
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ILanguageService _languageService;
        private readonly IItemService _itemService;
        private readonly IParserService _parserService;
        private readonly ITextParsers _textParsers;

        public TextsController(ILanguageService languageService, IItemService itemService, IParserService parserService, ITextParsers textParsers)
        {
            _languageService = languageService;
            _itemService = itemService;
            _parserService = parserService;
            _textParsers = textParsers;
        }

        private void InitParsers()
        {
            ViewBag.Parsers = _textParsers.FindAll().ToDictionary(x => x.TextParserId.ToString(), x => x.Name);
        }

        public ActionResult Index()
        {
            ViewBag.Languages = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageSimpleViewModel>>(_languageService.FindAllForOwner());

            return View();
        }

        #region texts
        [HttpGet]
        public ActionResult AddText()
        {
            InitParsers();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddText(TextModel model)
        {
            if(ModelState.IsValid)
            {
                var item = Mapper.Map<TextModel, Item>(model);
                item.ItemType = ItemType.Text;
                item.IsParallel = !string.IsNullOrWhiteSpace(item.L1Text);
                item.L1Text = model.L1Text;
                var tp = _textParsers.FindOne(model.ParseWith);
                item.ParseWith = tp == null ? ObjectId.Empty : tp.TextParserId;

                _itemService.Save(item);
                return this.RedirectToAction(x => x.EditText(item.ItemId.ToString())).Success("Text added");
            }

            InitParsers();
            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult AddMultiple()
        {
            var multiTextModel = new MultiTextModel();
            multiTextModel.Parts.Add(new MultiTextPartModel());
            InitParsers();
            return View(multiTextModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddMultiple(MultiTextModel model)
        {
            if(ModelState.IsValid)
            {
                Item item = new Item()
                                        {
                                            LanguageId = model.LanguageId,
                                            CollectionName = model.CollectionName,
                                            Tags = TagHelper.Split(model.Tags)
                                        };

                foreach(var part in model.Parts)
                {
                    var newItem = item.Clone();
                    newItem.Title = part.Title;
                    newItem.L1Text = part.Text;
                    newItem.L2Text = part.ParallelText;
                    newItem.Url = part.AudioUrl;
                    newItem.CollectionNo = part.CollectionNo;
                    newItem.ParallelIsRtl = part.ParallelIsRtl;
                    item.L1Text = part.Text;
                    var tp = _textParsers.FindOne(part.ParseWith);
                    item.ParseWith = tp == null ? ObjectId.Empty : tp.TextParserId;

                    newItem.ShareUrl = part.ShareUrl;

                    _itemService.Save(newItem);
                }

                return this.RedirectToAction(x => x.AddMultiple()).Success("Texts added");
            }

            InitParsers();
            return View(model).Error(Messages.FormValidationError);
        }

        public ActionResult AddTextPart()
        {
            InitParsers();
            return View("partial/multitextpart");
        }

        [HttpGet]
        [AutoMap(typeof(Item), typeof(TextModel))]
        public ActionResult EditText(string id)
        {
            var item = _itemService.FindOne(id);

            if(item == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Text not found");
            }

            int approximateWords = item.L1Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            ViewBag.TooManyWords = approximateWords > SystemSettings.Instance.Values.Site.MaxWordsParsingException;

            ViewBag.NextId = _itemService.NextItemId(item);
            ViewBag.PreviousId = _itemService.PreviousItemId(item);
            InitParsers();

            if(item.ParseWith != ObjectId.Empty)
            {
                ViewBag.CurrentlyParsing = true;
            }
            else
            {
                ViewBag.CurrentlyParsing = false;
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditText(string id, TextModel model)
        {
            var item = _itemService.FindOne(id);

            if(ModelState.IsValid)
            {
                TryUpdateModel(item, new[]
                                         {
                                             "Title", "CollectionName", "CollectionNo", "Url",
                                             "L2Text", "LanguageId", "ParallelIsRtl", "ShareUrl"
                                         });

                if(item.ParseWith == ObjectId.Empty)
                {
                    item.L1Text = model.L1Text;
                    var tp = _textParsers.FindOne(model.ParseWith);
                    item.ParseWith = tp == null ? ObjectId.Empty : tp.TextParserId;
                }

                item.Tags = TagHelper.Split(model.Tags);
                _itemService.Save(item);

                return this.RedirectToAction(x => x.EditText(item.ItemId.ToString())).Success("Text saved");
            }

            ViewBag.NextId = _itemService.NextItemId(item);
            ViewBag.PreviousId = _itemService.PreviousItemId(item);
            InitParsers();

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteText(string id)
        {
            _itemService.Delete(id);
            return this.RedirectToAction(x => x.Index()).Success("Text deleted");
        }

        [HttpGet]
        public ActionResult PdfText(string id)
        {
            var item = _itemService.FindOne(id);

            if(item == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Text not found");
            }

            var latexParserService = ObjectFactory.GetInstance<LatexParserService>();

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(item.LanguageId),
                    item,
                    wordService.FindAllForParsingSplit(item.LanguageId),
                    false
                );

            try
            {
                var parserOutput = latexParserService.Parse(parserInput);
                latexParserService.AddToQueue(UserId, parserOutput);
            }
            catch(TooManyWords e)
            {
                return this.RedirectToAction(x => x.EditText(id)).Error("There are too many words in your text to generate a PDF.");
            }

            return this.RedirectToAction(x => x.EditText(id)).Success("Your PDF has been placed in the queue. When it's done you will receive a message with a link.");
        }

        public ActionResult Download(string id)
        {
            var latexService = ObjectFactory.GetInstance<LatexParserService>();
            var buffer = latexService.FindOne(new ObjectId(id), UserId);

            if(buffer == null)
            {
                return this.RedirectToAction(x => x.FileNotFound());
            }

            return new FileContentResult(buffer, "application/pdf");
        }

        public ActionResult FileNotFound()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Split(string id)
        {
            var item = _itemService.FindOne(id);

            if(item == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Text not found");
            }

            ViewBag.NextId = _itemService.NextItemId(item);
            ViewBag.PreviousId = _itemService.PreviousItemId(item);
            var model = new SplitModel()
                            {
                                IsParallelText = item.IsParallel,
                                TextId = item.ItemId,
                                Title = item.Title,
                                StartingNumber = 1
                            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Split(string id, SplitModel model)
        {
            if(ModelState.IsValid)
            {
                _itemService.SplitItem(id, model.StartingNumber, model.Tags);
                return this.RedirectToAction(x => x.EditText(id)).Success("Text split");
            }

            var item = _itemService.FindOne(id);
            ViewBag.NextId = _itemService.NextItemId(item);
            ViewBag.PreviousId = _itemService.PreviousItemId(item);

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult AddFromFile()
        {
            JsonTextFromFile file = new JsonTextFromFile();
            file.Defaults = new JsonDefaults();
            file.Items = new JsonTextItem[1];
            file.Items[0] = new JsonTextItem();
            var json = JsonConvert.SerializeObject(file, Formatting.Indented);

            var languages = _languageService.FindAllForOwner();

            if(!languages.Any())
            {
                return this.RedirectToAction<LanguagesController>(x => x.Add()).Error("You must first add a language before you can add texts");
            }

            ViewBag.Languages = languages.Select(x => x.Name);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFromFile(TextFromFileModel model)
        {
            var languages = _languageService.FindAllForOwner();

            if(ModelState.IsValid)
            {
                try
                {
                    JsonTextFromFile json;

                    if(ZipFile.IsZipFile(model.File.InputStream, true))
                    {
                        model.File.InputStream.Position = 0;
                        using(var zip = ZipFile.Read(model.File.InputStream))
                        {
                            var data = zip[0];

                            if(data == null)
                            {
                                throw new FileNotFoundException();
                            }

                            using(var sr = new StreamReader(data.OpenReader()))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                json = serializer.Deserialize<JsonTextFromFile>(new JsonTextReader(sr));
                            }
                        }
                    }
                    else
                    {
                        using(var sr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            json = serializer.Deserialize<JsonTextFromFile>(new JsonTextReader(sr));
                        }
                    }

                    if(json == null) throw new NoNullAllowedException("File is empty");
                    if(json.Items == null || json.Items.Length == 0) throw new Exception("No texts are specified");

                    int result = _itemService.ImportFromFile(languages, json);

                    return this.RedirectToAction(x => x.AddFromFile()).Success(result + " texts were imported");
                }
                catch(Exception e)
                {
                    ModelState.AddModelError("File", e.Message);
                }
            }

            ViewBag.Languages = languages.Select(x => x.Name);
            return View(model).Error("Please correct the errors below");
        }
        #endregion

        #region videos
        [HttpGet]
        public ActionResult AddVideo()
        {
            var languages = _languageService.FindAllForOwner();

            if(!languages.Any())
            {
                return this.RedirectToAction<LanguagesController>(x => x.Add()).Error("You must first add a language before you can add a video");
            }

            ViewBag.Languages = languages.ToDictionary(x => x.LanguageId, x => x.Name);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddVideo(VideoModel model)
        {
            if(ModelState.IsValid)
            {
                var video = Mapper.Map<VideoModel, Item>(model);
                _itemService.Save(video);
                return this.RedirectToAction(x => x.EditVideo(video.ItemId.ToString())).Success("Video added");
            }

            ViewBag.Languages = _languageService.FindAllForOwner().ToDictionary(x => x.LanguageId, x => x.Name);
            return View().Error(Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(Item), typeof(VideoModel))]
        public ActionResult EditVideo(string id)
        {
            var video = _itemService.FindOne(id);

            if(video == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Video not found");
            }

            ViewBag.NextId = _itemService.NextItemId(video);
            ViewBag.PreviousId = _itemService.PreviousItemId(video);
            ViewBag.Languages = _languageService.FindAllForOwner().ToDictionary(x => x.LanguageId, x => x.Name);
            int approximateWords = video.L1Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            ViewBag.TooManyWords = approximateWords > SystemSettings.Instance.Values.Site.MaxWordsParsingException;

            return View(video);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditVideo(string id, VideoModel model)
        {
            var video = _itemService.FindOne(id);

            if(ModelState.IsValid)
            {
                TryUpdateModel(video, new[]
                                         {
                                             "Title", "CollectionName", "CollectionNo", "Url",
                                             "L1Text", "L2Text", "LanguageId"
                                         });

                video.Tags = TagHelper.Split(model.Tags);
                _itemService.Save(video);

                return this.RedirectToAction(x => x.EditVideo(video.ItemId.ToString())).Success("Video saved");
            }

            ViewBag.NextId = _itemService.NextItemId(video);
            ViewBag.PreviousId = _itemService.PreviousItemId(video);
            ViewBag.Languages = _languageService.FindAllForOwner().ToDictionary(x => x.LanguageId, x => x.Name);

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteVideo(string id)
        {
            _itemService.Delete(id);
            return this.RedirectToAction(x => x.Index()).Success("Video deleted");
        }
        #endregion

        #region view
        public ActionResult Read(string id)
        {
            ObjectId objectId;
            if(!ObjectId.TryParse(id, out objectId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");

            var item = _itemService.FindOne(objectId);

            if(item == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Text not found");
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(item.LanguageId),
                    item,
                    wordService.FindAllForParsingSplit(item.LanguageId),
                    false
                );

            try
            {
                var parserOutput = _parserService.Parse(parserInput);
                _itemService.MarkAsSeen(item);
                return View(parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }

        public ActionResult ReadParallel(string id)
        {
            ObjectId objectId;
            if(!ObjectId.TryParse(id, out objectId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");

            var item = _itemService.FindOne(objectId);

            if(item == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Text not found");
            }

            if(!item.IsParallel)
            {
                return this.RedirectToAction(x => x.Read(id)).Error("This is not a parallel text");
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(item.LanguageId),
                    item,
                    wordService.FindAllForParsingSplit(item.LanguageId),
                    true
                );

            try
            {
                var parserOutput = _parserService.Parse(parserInput);
                _itemService.MarkAsSeen(item);
                return View("read", parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }

        public ActionResult Watch(string id)
        {
            ObjectId objectId;
            if(!ObjectId.TryParse(id, out objectId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");
            var video = _itemService.FindOne(objectId);

            if(video == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Video not found");
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(video.LanguageId),
                    video,
                    wordService.FindAllForParsingSplit(video.LanguageId),
                    null
                );

            try
            {
                var parserOutput = _parserService.Parse(parserInput);
                _itemService.MarkAsSeen(video);
                return View(parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }
        #endregion
    }
}
