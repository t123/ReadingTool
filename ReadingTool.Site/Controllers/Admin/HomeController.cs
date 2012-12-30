using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Core;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Admin;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Controllers.Admin
{
    [CustomAuthorize(Roles = Constants.Roles.ADMIN)]
    public class HomeController : Controller
    {
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly ILanguageService _languageService;

        public HomeController(ISystemLanguageService systemLanguageService, ILanguageService languageService)
        {
            _systemLanguageService = systemLanguageService;
            _languageService = languageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PublicLanguages()
        {
            return View(_languageService.FindAllIncludePublic().Where(x => x.IsPublic).OrderBy(x => x.Name));
        }

        [HttpGet]
        public ActionResult AddPublicLanguage()
        {
            return View(new PublicLanguageViewModel { Settings = LanguageSettingsViewModel.Default });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPublicLanguage(PublicLanguageViewModel model)
        {
            if(ModelState.IsValid)
            {
                var sl = _systemLanguageService.FindByName(model.Name);

                _languageService.Save(
                    new Language()
                    {
                        Colour = "",
                        Name = model.Name,
                        IsPublic = true,
                        SystemLanguageId = sl == null ? (Guid?)null : sl.Id,
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
        public ActionResult EditPublicLanguage(Guid id)
        {
            var language = _languageService.Find(id);

            if(language == null)
            {
                this.FlashError(Constants.Messages.FORM_NOT_FOUND, DescriptionFormatter.GetDescription<Language>());
                return RedirectToAction("Index");
            }

            return View(new PublicLanguageViewModel
                {
                    Id = language.Id,
                    Name = language.Name,
                    Settings = Mapper.Map<LanguageSettings, LanguageSettingsViewModel>(language.Settings)
                }
                );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPublicLanguage(Guid id, PublicLanguageViewModel model)
        {
            var language = _languageService.Find(id);

            if(ModelState.IsValid)
            {
                language.SystemLanguageId = null;
                language.Name = model.Name;
                language.IsPublic = true;
                language.Settings = Mapper.Map<LanguageSettings>(model.Settings);
                _languageService.Save(language);

                this.FlashSuccess(Constants.Messages.FORM_UPDATE, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("EditPublicLanguage", new { id = id });
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return View(model);
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if(file != null && file.ContentLength > 0)
            {
                int count = 0;
                const short CodeColumnNo = 0;
                const short LanguageNameColumnNo = 6;

                try
                {
                    IList<SystemLanguage> languages = new List<SystemLanguage>();
                    string csv;
                    var currentLanguages = _systemLanguageService.FindAll().ToDictionary(x => x.Code);
                    using(TextReader tr = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        csv = tr.ReadToEnd();
                    }

                    int i = 0;
                    foreach(string line in csv.Split('\n'))
                    {
                        string[] split = line.Split('\t');

                        if(i++ == 0)
                        {
                            try
                            {
                                var ccode = split[CodeColumnNo];
                                var cname = split[LanguageNameColumnNo];

                                if(ccode != "Id" || cname != "Ref_Name")
                                    throw new Exception("Are you sure this is the right file?");
                            }
                            catch
                            {
                                throw new Exception("Are you sure this is the right file?");
                            }
                            continue;
                        }

                        string code = split[CodeColumnNo];
                        if(currentLanguages.ContainsKey(code)) continue;

                        if(code.Length != 3 || split[LanguageNameColumnNo].Length > 60)
                        {
                            throw new Exception(string.Format("{0}/{1}", code, split[LanguageNameColumnNo]));
                        }

                        languages.Add(new SystemLanguage()
                        {
                            Id = SequentialGuid.NewGuid(),
                            Code = code,
                            Name = split[LanguageNameColumnNo]
                        });

                        count++;
                    }

                    _systemLanguageService.Save(languages.OrderBy(x => x.Name).ToArray());

                    this.FlashSuccess("{0} languages imported", count);

                    return RedirectToAction("Import");
                }
                catch(Exception e)
                {
                    this.FlashError("Exception: " + e);
                    return View();
                }
            }

            this.FlashError("Please upload a file");
            return View();
        }
    }
}
