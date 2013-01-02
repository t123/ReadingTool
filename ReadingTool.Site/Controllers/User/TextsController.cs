using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Entities.Search;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Controllers.User
{
    public class TextsController : Controller
    {
        private readonly ITextService _textService;
        private readonly ILanguageService _languageService;
        private readonly IUserService _userService;
        private readonly IParserService _parserService;
        private readonly ITermService _termService;

        public TextsController(
            ITextService textService,
            ILanguageService languageService,
            IUserService userService,
            IParserService parserService,
            ITermService termService
        )
        {
            _textService = textService;
            _languageService = languageService;
            _userService = userService;
            _parserService = parserService;
            _termService = termService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AjaxRoute]
        public ActionResult IndexGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var searchResult = _textService.FilterTexts(new SearchOptions()
                {
                    Filter = filter,
                    Page = page ?? 1,
                    RowsPerPage = perPage ?? 15,
                    Sort = sort ?? "language",
                    Direction = sortDir
                });

            IList<TextListModel> textViewList = new List<TextListModel>();
            var languages = _languageService.FindAll().ToDictionary(x => x.Id);
            searchResult.Results.ToList().ForEach(x => textViewList.Add(
                new TextListModel
                    {
                        Id = x.Id,
                        CollectionName = x.CollectionName,
                        CollectionNo = x.CollectionNo,
                        HasAudio = !string.IsNullOrEmpty(x.AudioUrl),
                        IsParallel = x.IsParallel,
                        Language = languages.GetValueOrDefault(x.L1Id, new Language() { Name = "NA" }).Name,
                        LastSeen = x.LastSeen,
                        Title = x.Title,
                        LanguageColour = languages.GetValueOrDefault(x.L1Id, new Language() { Colour = "#FFFFFF" }).Colour,
                        Tags = x.Tags
                    }));

            SearchGridResult<TextListModel> result = new SearchGridResult<TextListModel>()
                {
                    Items = textViewList,
                    Page = page.Value,
                    Sort = sort,
                    Direction = sortDir,
                    RowsPerPage = perPage ?? 15,
                    TotalRows = searchResult.TotalRows
                };

            return PartialView("Partials/_list", result);
        }

        [HttpGet]
        public ActionResult Add()
        {
            ViewBag.Languages = _languageService.FindAll().OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(TextViewModel model, string save, string saveread)
        {
            if(!string.IsNullOrWhiteSpace(model.L2Text) && model.L2Id == null)
            {
                ModelState.AddModelError("L2Id", "Please choose a language");
            }

            var l1 = _languageService.Find(model.L1Id);
            if(l1 != null && l1.IsPublic)
            {
                ModelState.AddModelError("L1Id", "Please choose a language");
            }

            if(ModelState.IsValid)
            {
                Text t = new Text()
                    {
                        AudioUrl = model.AudioUrl,
                        CollectionName = model.CollectionName,
                        CollectionNo = model.CollectionNo,
                        L1Id = model.L1Id,
                        L2Id = model.L2Id,
                        L1Text = model.L1Text,
                        L2Text = model.L2Text,
                        Title = model.Title,
                        Tags = TagHelper.ToString(TagHelper.Split(model.Tags))
                    };

                _textService.Save(t);

                this.FlashSuccess(Constants.Messages.FORM_ADD, DescriptionFormatter.GetDescription(model));

                if(!string.IsNullOrEmpty(saveread))
                {
                    return RedirectToAction("Read", new { id = t.Id });
                }

                return RedirectToAction("Index");
            }

            ViewBag.Languages = _languageService.FindAll().OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);
            this.FlashError(Constants.Messages.FORM_FAIL);

            return View();
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Text>());
                return RedirectToAction("Index");
            }

            var languages = _languageService.FindAllIncludePublic();
            ViewBag.L1Languages = languages.Where(x => !x.IsPublic).OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);
            ViewBag.L2Languages = languages.OrderByDescending(x => x.IsPublic).ThenBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);

            return View(new TextViewModel()
                {
                    AudioUrl = text.AudioUrl,
                    CollectionName = text.CollectionName,
                    CollectionNo = text.CollectionNo,
                    Id = text.Id,
                    L1Id = text.L1Id,
                    L2Id = text.L2Id,
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    Tags = text.Tags,
                    Title = text.Title
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, TextViewModel model, string save, string saveread)
        {
            if(!string.IsNullOrWhiteSpace(model.L2Text) && model.L2Id == null)
            {
                ModelState.AddModelError("L2Id", "Please choose a language");
            }

            var l1 = _languageService.Find(model.L1Id);
            if(l1 != null && l1.IsPublic)
            {
                ModelState.AddModelError("L1Id", "Please choose a language");
            }

            if(ModelState.IsValid)
            {
                Text text = _textService.Find(id);

                text.AudioUrl = model.AudioUrl;
                text.CollectionName = model.CollectionName;
                text.CollectionNo = model.CollectionNo;
                text.L1Id = model.L1Id;
                text.L2Id = model.L2Id;
                text.L1Text = model.L1Text;
                text.L2Text = model.L2Text;
                text.Tags = TagHelper.ToString(TagHelper.Split(model.Tags));
                text.Title = model.Title;

                _textService.Save(text);
                this.FlashSuccess(Constants.Messages.FORM_UPDATE, DescriptionFormatter.GetDescription(model));

                if(!string.IsNullOrEmpty(saveread))
                {
                    return RedirectToAction("Read", new { id = id });
                }

                return RedirectToAction("Edit", new { id = id });
            }

            var languages = _languageService.FindAllIncludePublic();
            ViewBag.L1Languages = languages.Where(x => !x.IsPublic).OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);
            ViewBag.L2Languages = languages.OrderByDescending(x => x.IsPublic).ThenBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);
            this.FlashError(Constants.Messages.FORM_FAIL);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            _textService.Delete(_textService.Find(id));
            this.FlashSuccess(Constants.Messages.FORM_DELETE, DescriptionFormatter.GetDescription<Text>());

            return RedirectToAction("Index");
        }

        public ActionResult Read(Guid id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Text>());
                return RedirectToAction("Index");
            }

            return View(Create(text, false));
        }

        public ActionResult ReadInParallel(Guid id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Text>());
                return RedirectToAction("Index");
            }

            return View("Read", Create(text, true));
        }

        [HttpGet]
        public ActionResult Import()
        {
            var languages = _languageService.FindAll().Select(x => x.Name).OrderBy(x => x).ToArray();
            ViewBag.Languages = languages;
            ViewBag.SampleJson = TempData["SampleJson"] ?? "";
            ViewBag.SampleJsonModel = TempData["SampleJsonModel"] ?? null;

            return View();
        }

        public ActionResult CreateSample(JsonSampleModel model)
        {
            if(ModelState.IsValid)
            {
                TextImport ti = new TextImport()
                    {
                        Defaults = new TextImport.JsonDefaults()
                            {
                                AutoNumberCollection = model.AutoNumberCollection,
                                CollectionName = model.CollectionName,
                                L1LanguageName = model.L1Name,
                                L2LanguageName = model.L2Name,
                                StartCollectionWithNumber = model.StartCollectionWithNumber,
                                Tags = model.Tags
                            },
                    };

                ti.Items = new TextImport.JsonTextItem[model.NumberOfItems ?? 1];
                for(int i = 0; i < (model.NumberOfItems ?? 1); i++)
                {
                    ti.Items[i] = new TextImport.JsonTextItem()
                        {
                            L1LanguageName = "",
                            L2LanguageName = "",
                            Title = "",
                            AudioUrl = "",
                            CollectionName = "",
                            Tags = "",
                            L1Text = "",
                            L2Text = "",
                        };
                };

                TempData["SampleJson"] = JsonConvert.SerializeObject(ti, Formatting.Indented);
                this.FlashSuccess("Your sample is below. Please make sure your editor has UTF-8 encoding.");
            }
            else
            {
                this.FlashError(Constants.Messages.FORM_FAIL);
            }

            TempData["SampleJsonModel"] = model;
            return RedirectToAction("Import");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(TextImportModel model)
        {
            ViewBag.Languages = _languageService.FindAll().Select(x => x.Name).OrderBy(x => x).ToArray();

            if(ModelState.IsValid && model.File != null && model.File.ContentLength > 0)
            {
                //TODO allow zip

                try
                {
                    TextImport json;
                    using(var sr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        json = serializer.Deserialize<TextImport>(new JsonTextReader(sr));

                        if(json == null) throw new Exception("File is empty");
                        if(json.Items == null || json.Items.Length == 0) throw new Exception("No texts are specified");
                    }

                    int imported = _textService.Import(json);

                    this.FlashSuccess("{0} texts were imported", imported);
                    return this.RedirectToAction("Import");
                }
                catch(JsonReaderException e)
                {
                    this.FlashError("Invalid JSON file");
                }
                catch(Exception e)
                {
                    this.FlashError(e.Message);
                }

                return View(model);
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return View(model);
        }

        private ReadViewModel Create(Text text, bool asParallel)
        {
            var l1Language = _languageService.Find(text.L1Id);
            var l2Language = _languageService.Find(text.L2Id ?? Guid.Empty);
            var terms = _termService.FindAllForParsing(l1Language);
            var parsed = _parserService.Parse(asParallel, l1Language, l2Language, terms, text);

            var model = new ReadViewModel()
            {
                Text = text,
                Language = l1Language,
                ParsedText = parsed,
                AsParallel = asParallel,
                User = _userService.Find(this.CurrentUserId()),
                PagedTexts = _textService.FindPagedTexts(text),
            };

            return model;
        }

        [AjaxRoute]
        public ActionResult PerformAction(string action, Guid[] ids, string input)
        {
            try
            {
                if(ids == null || ids.Length == 0)
                {
                    return new JsonNetResult() { Data = "OK" };
                }

                IList<Text> texts = new List<Text>();
                ids.ToList().ForEach(x => texts.Add(_textService.Find(x)));

                switch(action)
                {
                    case "add":
                        if(!string.IsNullOrWhiteSpace(input))
                        {
                            foreach(var t in texts)
                            {
                                t.Tags = TagHelper.ToString(TagHelper.Merge(TagHelper.Split(t.Tags), TagHelper.Split(input)));
                                _textService.Save(t);
                            }
                        }
                        break;

                    case "remove":
                        if(!string.IsNullOrWhiteSpace(input))
                        {
                            foreach(var t in texts)
                            {
                                t.Tags = TagHelper.ToString(TagHelper.Remove(TagHelper.Split(t.Tags), TagHelper.Split(input)));
                                _textService.Save(t);
                            }
                        }
                        break;

                    case "delete":
                        foreach(var t in texts)
                        {
                            _textService.Delete(t);
                        }
                        break;

                    case "rename":
                        foreach(var t in texts)
                        {
                            t.CollectionName = input;
                            _textService.Save(t);
                        }
                        break;
                }

                return new JsonNetResult() { Data = "OK" };
            }
            catch(Exception e)
            {
                return new JsonNetResult() { Data = "FAIL" };
            }
        }
    }
}
