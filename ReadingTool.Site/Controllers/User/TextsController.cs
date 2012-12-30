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
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Services;
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
            return View(_textService.FindAll());
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
            var languageName = languages.Length > 0 ? languages[0] : string.Empty;

            TextImport ti = new TextImport()
                {
                    Defaults = new TextImport.JsonDefaults()
                        {
                            L1LanguageName = languageName,
                            L2LanguageName = languageName,
                        },
                    Items = new TextImport.JsonTextItem[2]
                        {
                            new TextImport.JsonTextItem() { L1LanguageName = languageName, L2LanguageName = languageName},
                            new TextImport.JsonTextItem() { L1LanguageName = languageName, L2LanguageName = languageName},
                        }
                };

            ViewBag.SampleJson = JsonConvert.SerializeObject(ti, Formatting.Indented);
            ViewBag.Languages = languages;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(TextImportModel model)
        {
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
                finally
                {
                    ViewBag.Languages = _languageService.FindAll().Select(x => x.Name).OrderBy(x => x);
                }

                return View(model);
            }

            ViewBag.Languages = _languageService.FindAll().Select(x => x.Name).OrderBy(x => x);
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
                //KeyBindings = Mapper.Map<KeyBindings, KeyBindingsModel>(user.KeyBindings),
            };

            return model;
        }
    }
}
