using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Api.Attributes;
using ReadingTool.Api.Models.Languages;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Api.Controllers
{
    [NeedsPersistence]
    [System.Web.Http.Authorize]
    public class LanguagesController : ApiController
    {
        private long UserId
        {
            get { return long.Parse(HttpContext.Current.User.Identity.Name); }
        }

        private readonly Repository<Language> _languageRepository;
        private readonly Repository<User> _userRepository;

        public LanguagesController(Repository<Language> languageRepository, Repository<User> userRepository)
        {
            _languageRepository = languageRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<LanguageResponseModel> Get()
        {
            var languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId))
                .OrderBy(x => x.Name);

            return Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageResponseModel>>(languages);
        }

        public LanguageResponseModel Get(long id)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User == _userRepository.LoadOne(UserId));

            if(language == null)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = string.Format("Language {0} not found", id)
                });
            }

            return Mapper.Map<Language, LanguageResponseModel>(language);
        }
    }
}
