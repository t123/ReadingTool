#region License
// ReviewController.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.Review;
using ReadingTool.Site.Models.Terms;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class ReviewController : Controller
    {
        private readonly Repository<User> _userRepository;
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Language> _languageRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private long UserId
        {
            get { return long.Parse(HttpContext.User.Identity.Name); }
        }

        public ReviewController(Repository<User> userRepository, Repository<Term> termRepository, Repository<Language> languageRepository)
        {
            _userRepository = userRepository;
            _termRepository = termRepository;
            _languageRepository = languageRepository;
        }

        [HttpGet]
        public ActionResult Index(long? id)
        {
            IQueryable<Term> terms;
            if(id == null || id == 0)
            {
                terms = _termRepository.FindAll(x => x.User.UserId == UserId && x.State == TermState.NotKnown && x.NextReview < DateTime.Now);

            }
            else
            {
                terms = _termRepository.FindAll(x => x.Language.LanguageId == id.Value && x.User.UserId == UserId && x.State == TermState.NotKnown && x.NextReview < DateTime.Now);
            }

            ViewBag.ReviewTotal = terms.Count();
            terms = terms.OrderBy(x => x.NextReview).Take(10);
            ViewBag.Id = id;

            var model = new ReviewModel
                {
                    ReviewTotal = terms.Count(),
                    Terms = Mapper.Map<IEnumerable<Term>, IEnumerable<TermViewModel>>(terms),
                    LanguageId = id ?? 0,
                    Languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name)
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index()
        {
            Guid? id = Guid.Parse(Request.Form["LanguageId"]);

            foreach(var key in Request.Form.AllKeys.Where(x => x.StartsWith("term_")))
            {
                long termId;

                if(!long.TryParse(key.Remove(0, 5), out termId))
                {
                    continue;
                }

                var term = _termRepository.FindOne(x => x.TermId == termId && x.User == _userRepository.LoadOne(UserId));

                if(term == null)
                {
                    continue;
                }

                if(Request.Form[key] == "skip")
                {
                    term.NextReview = (term.NextReview ?? DateTime.Now).AddMinutes(10);
                    _termRepository.Save(term);
                }
                else if(Request.Form[key] == "know")
                {
                    term.Box++;
                }
                else
                {
                    term.Box = 1;
                }

                var newState = Term.NextReviewDate(term);
                term.State = newState.Item1;
                term.NextReview = newState.Item2;

                _termRepository.Save(term);
            }

            return RedirectToAction("Index", new { id = id });
        }
    }
}
