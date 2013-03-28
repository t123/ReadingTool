using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Search;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.Terms;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class TermsController : Controller
    {
        private readonly Repository<User> _userRepository;
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<Tag> _tagRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private long UserId
        {
            get { return long.Parse(HttpContext.User.Identity.Name); }
        }

        public TermsController(
            Repository<User> userRepository,
            Repository<Term> termRepository,
            Repository<Language> languageRepository,
            Repository<Tag> tagRepository
            )
        {
            _userRepository = userRepository;
            _termRepository = termRepository;
            _languageRepository = languageRepository;
            _tagRepository = tagRepository;
        }

        public ActionResult Index()
        {
            ViewBag.Languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name);
            return View();
        }

        public PartialViewResult IndexGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var so = new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? SearchGridPaging.DefaultRows,
                Sort = sort ?? "language",
                Direction = sortDir
            };

            var terms = _termRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId));
            var filterTerms = SearchFilterParser.Parse(filter);
            var languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).ToDictionary(x => x.Name.ToLowerInvariant(), x => x.LanguageId);

            foreach(var term in filterTerms.Tags)
            {
                if(term == "known")
                {
                    terms = terms.Where(x => x.State == TermState.Known);
                }
                else if(term == "notknown")
                {
                    terms = terms.Where(x => x.State == TermState.NotKnown);
                }
                else if(term == "ignore")
                {
                    terms = terms.Where(x => x.State == TermState.Ignore);
                }
                else if(term == "notseen")
                {
                    terms = terms.Where(x => x.State == TermState.NotSeen);
                }
                else
                {
                    terms = terms.Where(x => x.Tags.Any(y => y.TagTerm == term));
                }
            }

            foreach(var term in filterTerms.Other)
            {
                if(languages.ContainsKey(term))
                {
                    terms = terms.Where(x => x.Language.LanguageId == languages[term]);
                }
                else
                {
                    terms = terms.Where(x => x.Phrase.StartsWith(term));
                }
            }

            var count = terms.Count();

            switch(so.Sort)
            {
                case "box":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        terms = terms.OrderBy(x => x.Box).ThenBy(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    else
                    {
                        terms = terms.OrderByDescending(x => x.Box).ThenBy(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    break;

                case "phrase":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        terms = terms.OrderBy(x => x.Phrase).ThenBy(x => x.Language.Name);
                    }
                    else
                    {
                        terms = terms.OrderByDescending(x => x.Phrase).ThenBy(x => x.Language.Name);
                    }
                    break;

                case "state":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        terms = terms.OrderBy(x => x.State).ThenBy(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    else
                    {
                        terms = terms.OrderByDescending(x => x.State).ThenBy(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    break;

                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        terms = terms.OrderBy(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    else
                    {
                        terms = terms.OrderByDescending(x => x.Language.Name).ThenBy(x => x.Phrase);
                    }
                    break;
            }

            terms = terms.Skip(so.Skip).Take(so.RowsPerPage);

            var searchResult = new SearchResult<TermViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Term>, IEnumerable<TermViewModel>>(terms),
                TotalRows = count
            };

            var result = new SearchGridResult<TermViewModel>()
            {
                Items = searchResult.Results,
                Paging = new SearchGridPaging()
                {
                    Page = so.Page,
                    TotalRows = searchResult.TotalRows,
                    RowsPerPage = perPage ?? SearchGridPaging.DefaultRows
                },
                Direction = sortDir,
                Sort = sort
            };

            return PartialView("Partials/_grid", result);
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            var term = _termRepository.FindOne(x => x.TermId == id && x.User == _userRepository.LoadOne(UserId));

            if(term == null)
            {
                return RedirectToAction("Index");
            }

            var model = Mapper.Map<Term, TermModel>(term);

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(long id, TermModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var term = _termRepository.FindOne(x => x.TermId == id && x.User.UserId == UserId);

            if(term == null || id != model.TermId)
            {
                return RedirectToAction("Index");
            }

            term.BasePhrase = model.BasePhrase.Trim();
            term.Definition = model.Definition.Trim();
            term.Modified = DateTime.Now;
            term.Sentence = model.Sentence.Trim();

            if(term.State != model.State)
            {
                term.State = model.State;
                term.NextReview = Term.NextReviewDate(term).Item2;
            }

            term.Tags.Clear();
            foreach(var tag in Tags.ToTags(model.Tags))
            {
                var existing = _tagRepository.FindOne(x => x.TagTerm.Equals(tag.ToLowerInvariant()));

                if(existing == null)
                {
                    existing = new Tag()
                        {
                            TagTerm = tag
                        };
                }

                term.Tags.Add(existing);
            }

            //term.HasTags = term.Tags.Count > 0;
            _termRepository.Save(term);

            return RedirectToAction("Edit", new { id = id });
        }

        public FileContentResult Export(long id, bool all)
        {
            IEnumerable<Term> terms;

            if(id == 0)
            {
                if(all)
                {
                    terms = _termRepository
                        .FindAll(x => x.User == _userRepository.LoadOne(UserId))
                        .OrderBy(x => x.Language.Name)
                        .ThenBy(x => x.Phrase);
                }
                else
                {
                    terms = _termRepository
                        .FindAll(x => x.User == _userRepository.LoadOne(UserId) && x.State == TermState.NotKnown)
                        .OrderBy(x => x.Language.Name)
                        .ThenBy(x => x.Phrase);
                }
            }
            else
            {
                if(all)
                {
                    terms = _termRepository
                        .FindAll(x => x.User == _userRepository.LoadOne(UserId) && x.Language == _languageRepository.LoadOne(id))
                        .OrderBy(x => x.Language.Name)
                        .ThenBy(x => x.Phrase);
                }
                else
                {
                    terms = _termRepository
                        .FindAll(x => x.User == _userRepository.LoadOne(UserId) && x.Language == _languageRepository.LoadOne(id) && x.State == TermState.NotKnown)
                        .OrderBy(x => x.Language.Name)
                        .ThenBy(x => x.Phrase);
                }
            }

            StringBuilder csv = new StringBuilder();

            foreach(var term in terms)
            {
                csv.AppendFormat("{0}\t", term.TermId);
                csv.AppendFormat("{0}\t", term.State);
                csv.AppendFormat("{0}\t", term.Phrase);
                csv.AppendFormat("{0}\t", term.BasePhrase);
                csv.AppendFormat("{0}\t", term.Sentence);
                csv.AppendFormat("{0}\t", term.Definition.Replace("\n", "<br/>"));
                csv.AppendFormat("{0}\t", String.Join(" ", term.Tags.Select(x => x.TagTerm)));

                csv.AppendFormat("{0}\t", term.Created);
                csv.AppendFormat("{0}\t", term.Modified);
                csv.AppendFormat("{0}\t", term.Length);
                csv.AppendFormat("{0}\t", term.Box);
                csv.AppendFormat("{0}\t", term.Language.LanguageId);
                csv.AppendFormat("{0}\t", term.NextReview);
                csv.AppendFormat("{0}\t", term.Text.TextId);
                csv.Append("\n");
            }

            csv.Remove(csv.Length - 1, 1);
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/plain", "words.tsv");
        }
    }
}