#region License
// TermsController.cs is part of ReadingTool.Api
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Api.Attributes;
using ReadingTool.Api.Models.Terms;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Api.Controllers
{
    [NeedsPersistence]
    [System.Web.Http.Authorize]
    public class TermsController : ApiController
    {
        private readonly Repository<Term> _termRepository;
        private readonly Repository<User> _userRepository;

        private long UserId
        {
            get { return long.Parse(HttpContext.Current.User.Identity.Name); }
        }

        public TermsController(
            Repository<Term> termRepository,
            Repository<User> userRepository
            )
        {
            _termRepository = termRepository;
            _userRepository = userRepository;
        }

        // GET api/terms
        public IEnumerable<TermResponseModel> Get(int page = 1, string state = "all", Guid? language = null)
        {
            if(page < 1)
            {
                page = 1;
            }

            var terms = _termRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId));

            switch(state.ToLowerInvariant())
            {
                case "known":
                    terms = terms.Where(x => x.State == TermState.Known);
                    break;

                case "notknown":
                    terms = terms.Where(x => x.State == TermState.NotKnown);
                    break;

                case "notseen":
                    terms = terms.Where(x => x.State == TermState.NotSeen);
                    break;

                case "ignore":
                    terms = terms.Where(x => x.State == TermState.Ignore);
                    break;

                default:
                case "all":
                    break;
            }

            if(language != null && language != Guid.Empty)
            {
                terms = terms.Where(x => x.Language.LanguageId == language);
            }

            terms = terms.OrderBy(x => x.Language.Name).ThenBy(x => x.PhraseLower);
            terms = terms.Skip((page - 1) * 250).Take(250);

            return Mapper.Map<IEnumerable<Term>, IEnumerable<TermResponseModel>>(terms);
        }

        // GET api/terms/5
        public TermResponseModel Get(Guid id)
        {
            var term = _termRepository.FindOne(x => x.TermId == id && x.User == _userRepository.LoadOne(UserId));

            if(term == null)
            {
                throw new HttpResponseException(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        ReasonPhrase = string.Format("Term {0} not found", id)
                    });
            }

            return Mapper.Map<Term, TermResponseModel>(term);
        }
    }
}
