using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Entities;
using ReadingTool.Site.Attributes;
using ReadingTool.Repository;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Admin;

namespace ReadingTool.Site.Controllers.Home
{
    [CustomAuthorizeAttribute(Roles = "ADMIN")]
    [NeedsPersistence]
    public class AdminController : Controller
    {
        private readonly Repository<SystemLanguage> _systemLanguageRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger("Admin");

        public AdminController(Repository<SystemLanguage> systemLanguageRepository)
        {
            _systemLanguageRepository = systemLanguageRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AutoMap(typeof(IEnumerable<SystemLanguage>), typeof(IEnumerable<SystemLanguageIndexModel>))]
        public ActionResult Languages()
        {
            return View(_systemLanguageRepository.FindAll().OrderBy(x => x.Name));
        }

        [HttpGet]
        public ActionResult AddLanguage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLanguage(SystemLanguageModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            SystemLanguage sl = new SystemLanguage()
                {
                    Code = model.Code,
                    Name = model.Name
                };

            _systemLanguageRepository.Save(sl);
            _logger.InfoFormat("New Language {0}/{1} added", sl.Name, sl.Code);

            this.FlashSuccess("System language added.");
            return RedirectToAction("Languages");
        }

        [HttpGet]
        public ActionResult EditLanguage(int id)
        {
            var sl = _systemLanguageRepository.FindOne(id);

            if(sl == null)
            {
                return RedirectToAction("Languages");
            }

            return View(new SystemLanguageModel
                {
                    Code = sl.Code,
                    Name = sl.Name,
                    SystemLanguageId = sl.SystemLanguageId
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLanguage(SystemLanguageModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var sl = _systemLanguageRepository.FindOne(model.SystemLanguageId);
            _logger.InfoFormat("Edit Language from {0}/{1} to {2}/{3}", sl.Name, sl.Code, model.Name, model.Code);

            sl.Code = model.Code;
            sl.Name = model.Name;
            _systemLanguageRepository.Save(sl);

            this.FlashSuccess("System language updated.");
            return RedirectToAction("Languages");
        }
    }
}
