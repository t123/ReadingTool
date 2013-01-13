using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using MongoDB.Bson;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Controllers.User
{
    [Authorize(Roles = Constants.Roles.WEB)]
    public class LanguagesController : Controller
    {
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;

        public LanguagesController(ILanguageService languageService, ISystemLanguageService systemLanguageService)
        {
            _languageService = languageService;
            _systemLanguageService = systemLanguageService;
        }

        public ActionResult Index()
        {
            var stats = _languageService.FindBasicLanguageStats();
            ViewBag.TextStats = stats.Item1;
            ViewBag.TermStats = stats.Item2;

            return View(_languageService.FindAll());
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View(new LanguageViewModel { Settings = LanguageSettingsViewModel.Default });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(LanguageViewModel model)
        {
            if(ModelState.IsValid)
            {
                var sl = _systemLanguageService.FindByName(model.Name);

                _languageService.Save(
                    new Language()
                        {
                            Colour = model.Colour,
                            Name = model.Name,
                            SystemLanguageId = sl == null ? (ObjectId?)null : sl.Id,
                            Settings = Mapper.Map<LanguageSettings>(model.Settings)
                        }
                    );

                this.FlashSuccess(Constants.Messages.FORM_ADD, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("Index");
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return View();
        }

        [HttpGet]
        public ActionResult Edit(ObjectId id)
        {
            var language = _languageService.FindOne(id);

            if(language == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Language>());
                return RedirectToAction("Index");
            }

            return View(new LanguageViewModel
                {
                    Id = language.Id,
                    Colour = language.Colour,
                    Name = language.Name,
                    SystemLanguage = language.SystemLanguageId.HasValue ? _systemLanguageService.FindOne(language.SystemLanguageId.Value).Name : "",
                    Settings = Mapper.Map<LanguageSettings, LanguageSettingsViewModel>(language.Settings)
                }
                );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ObjectId id, LanguageViewModel model)
        {
            var language = _languageService.FindOne(id);

            if(ModelState.IsValid)
            {
                var sl = _systemLanguageService.FindByName(model.SystemLanguage);
                language.SystemLanguageId = sl == null ? (ObjectId?)null : sl.Id;
                language.Name = model.Name;
                language.Colour = model.Colour;
                language.Settings = Mapper.Map<LanguageSettings>(model.Settings);
                _languageService.Save(language);

                this.FlashSuccess(Constants.Messages.FORM_UPDATE, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("Edit", new { id = id });
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ObjectId id)
        {
            Language l = _languageService.FindOne(id);
            _languageService.Delete(l);

            this.FlashSuccess(Constants.Messages.FORM_DELETE, DescriptionFormatter.GetDescription<Language>() + " " + l.Name);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Dictionaries(ObjectId id)
        {
            Language language = _languageService.FindOne(id);

            if(language == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Language>());
                return RedirectToAction("Index");
            }

            return View(language);
        }

        [HttpGet]
        public ActionResult AddDictionary(ObjectId id)
        {
            Language language = _languageService.FindOne(id);

            if(language == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Language>());

                return RedirectToAction("Index");
            }

            return View(new DictionaryViewModel() { LanguageId = language.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDictionary(ObjectId id, DictionaryViewModel model)
        {
            Language language = _languageService.FindOne(id);

            if(ModelState.IsValid)
            {
                LanguageDictionary ld = Mapper.Map<DictionaryViewModel, LanguageDictionary>(model);
                ld.Id = ObjectId.GenerateNewId();
                ld.DisplayOrder = 1000;
                language.Dictionaries.Add(ld);

                _languageService.Save(language);

                this.FlashSuccess(Constants.Messages.FORM_ADD, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("Dictionaries", new { id = id });
            }

            this.FlashError(Constants.Messages.FORM_FAIL);

            return View(new DictionaryViewModel() { LanguageId = language.Id });
        }

        [HttpGet]
        public ActionResult EditDictionary(ObjectId id, ObjectId dictionaryId)
        {
            Language language = _languageService.FindOne(id);

            if(language == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Language>());
                return RedirectToAction("Index");
            }

            var dictionary = language.Dictionaries.FirstOrDefault(x => x.Id == dictionaryId);

            if(dictionary == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<LanguageDictionary>());
                return RedirectToAction("Dictionaries", new { id = id });
            }

            var mapped = Mapper.Map<LanguageDictionary, DictionaryViewModel>(dictionary);
            mapped.LanguageId = language.Id;

            return View(mapped);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDictionary(ObjectId id, ObjectId dictionaryId, DictionaryViewModel model)
        {
            Language language = _languageService.FindOne(id);

            if(ModelState.IsValid)
            {
                var dictionary = language.Dictionaries.FirstOrDefault(x => x.Id == dictionaryId);
                dictionary.Name = model.Name;
                dictionary.AutoOpen = model.AutoOpen;
                dictionary.Parameter = model.Parameter;
                dictionary.Url = model.Url;
                dictionary.UrlEncoding = model.UrlEncoding;
                dictionary.WindowName = model.WindowName;

                _languageService.Save(language);

                this.FlashSuccess(Constants.Messages.FORM_UPDATE, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("EditDictionary", new { id = id, dictionaryId = dictionaryId });
            }

            this.FlashError(Constants.Messages.FORM_FAIL);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDictionary(ObjectId id, ObjectId dictionaryId)
        {
            Language language = _languageService.FindOne(id);

            if(language != null)
            {
                language.Dictionaries.Remove(language.Dictionaries.FirstOrDefault(x => x.Id == dictionaryId));
                _languageService.Save(language);
            }

            this.FlashSuccess(Constants.Messages.FORM_DELETE, DescriptionFormatter.GetDescription<LanguageDictionary>());
            return RedirectToAction("Dictionaries", new { id = id });
        }

        [AjaxRoute]
        public JsonResult AutoCompleteSystemLanguage(string query)
        {
            var l = _systemLanguageService.FindAllStartingWith(query);
            dynamic response = new
                {
                    query = query,
                    suggestions = l.Select(x => x.Name).ToArray()
                };

            return new JsonNetResult() { Data = response };
        }

        [AjaxRoute]
        public JsonResult UpdateOrder(ObjectId languageId, IEnumerable<ObjectId> ids)
        {
            try
            {
                //if(!string.IsNullOrEmpty(ids))
                if(ids != null && ids.Any())
                {
                    var l = _languageService.FindOne(languageId);

                    if(l == null)
                    {
                        throw new Exception("Language is null");
                    }

                    //Guid[] guids = JsonConvert.DeserializeObject<Guid[]>(ids);
                    int counter = 1;
                    foreach(var did in ids)
                    {
                        var dictionary = l.Dictionaries.FirstOrDefault(x => x.Id == did);

                        if(dictionary == null)
                        {
                            continue;
                        }

                        dictionary.DisplayOrder = counter++;
                    }

                    _languageService.Save(l);
                }

                return new JsonNetResult() { Data = "OK", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch
            {
                return new JsonNetResult() { Data = "FAIL", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
    }
}
