using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Models.WebApi;

namespace ReadingTool.Site.Controllers.Api
{
    public class LanguagesController : ApiController
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public IEnumerable<LanguageModel> GetLanguages()
        {
            return Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageModel>>(_languageService.FindAll());
        }

        public LanguageModel GetLanguageById(Guid id)
        {
            return Mapper.Map<Language, LanguageModel>(_languageService.Find(id));
        }
    }
}
