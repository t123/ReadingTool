using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Api.Attributes;
using ReadingTool.Api.Models.Terms;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Services;

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
        public IEnumerable<TermResponseModel> Get(int page = 1, string state = "all", int language = 0)
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

            if(language > 0)
            {
                terms = terms.Where(x => x.Language.LanguageId == language);
            }

            terms = terms.OrderBy(x => x.Language.Name).ThenBy(x => x.PhraseLower);
            terms = terms.Skip((page - 1) * 250).Take(250);

            return Mapper.Map<IEnumerable<Term>, IEnumerable<TermResponseModel>>(terms);
        }

        // GET api/terms/5
        public TermResponseModel Get(long id)
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
