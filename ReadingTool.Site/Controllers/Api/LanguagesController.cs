using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.Site.Controllers.Api
{
    public class LanguagesController : ApiController
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpGet, HttpPost]
        public IEnumerable<Language> Index()
        {
            return _languageService.FindAll();
        }
    }
}
