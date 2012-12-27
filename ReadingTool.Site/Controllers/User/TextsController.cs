using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core;
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
        public ActionResult Add(TextViewModel model)
        {
            if(!string.IsNullOrWhiteSpace(model.L2Text) && model.L2Id == null)
            {
                ModelState.AddModelError("L2Id", "Please choose a language");
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
                        Tags = TagHelper.Split(model.Tags),
                        Title = model.Title
                    };

                _textService.Save(t);
                this.FlashSuccess("Text added");
                return RedirectToAction("Index");
            }

            ViewBag.Languages = _languageService.FindAll().OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);

            return View();
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError("Text not found");
                return RedirectToAction("Index");
            }

            ViewBag.Languages = _languageService.FindAll().OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);

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
                    Tags = TagHelper.ToString(text.Tags),
                    Title = text.Title
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, TextViewModel model)
        {
            if(!string.IsNullOrWhiteSpace(model.L2Text) && model.L2Id == null)
            {
                ModelState.AddModelError("L2Id", "Please choose a language");
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
                text.Tags = TagHelper.Split(model.Tags);
                text.Title = model.Title;

                _textService.Save(text);
                this.FlashSuccess("Text updated");
                return RedirectToAction("Edit", new { id = id });
            }

            ViewBag.Languages = _languageService.FindAll().OrderBy(x => x.Name).ToDictionary(x => x.Id, x => x.Name);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            _textService.Delete(_textService.Find(id));
            this.FlashSuccess("Text deleted");

            return RedirectToAction("Index");
        }

        public ActionResult Read(long id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError("Text not found");
                return RedirectToAction("Index");
            }

            return View(Create(text, false));
        }

        public ActionResult ReadInParallel(long id)
        {
            Text text = _textService.Find(id);

            if(text == null)
            {
                this.FlashError("Text not found");
                return RedirectToAction("Index");
            }

            return View("Read", Create(text, true));
        }

        private ReadViewModel Create(Text text, bool asParallel)
        {
            var l1Language = _languageService.Find(text.L1Id);
            var l2Language = _languageService.Find(text.L2Id ?? -1);
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
