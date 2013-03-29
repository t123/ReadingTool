#region License
// LanguagesController.cs is part of ReadingTool.Api
// 
// ReadingTool.Api is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Api is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Api. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

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
