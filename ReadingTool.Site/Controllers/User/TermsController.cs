using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Entities.Search;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Controllers.User
{
    public class TermsController : Controller
    {
        private readonly ITermService _termService;
        private readonly ILanguageService _languageService;

        public TermsController(ITermService termService, ILanguageService languageService)
        {
            _termService = termService;
            _languageService = languageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AjaxRoute]
        public ActionResult IndexGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var searchResult = _termService.FilterTerms(new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? 15,
                Sort = sort ?? "language",
                Direction = sortDir
            });

            foreach(var t in searchResult.Results)
            {
                t.AddIndividualTerms(_termService.FindIndividualTerms(t));
            }

            IList<TermListModel> termViewList = new List<TermListModel>();
            var languages = _languageService.FindAll().ToDictionary(x => x.Id);
            searchResult.Results.ToList().ForEach(x => termViewList.Add(
                new TermListModel
                    {
                        Box = x.Box,
                        Id = x.Id,
                        Language = languages.GetValueOrDefault(x.LanguageId, new Language() { Name = "NA" }).Name,
                        LanguageColour = languages.GetValueOrDefault(x.LanguageId, new Language() { Colour = "#FFFFFF" }).Colour,
                        NextReview = x.NextReview,
                        TermPhrase = x.TermPhrase,
                        State = x.State.ToDescription(),
                        Definition = x.Definition,
                        IndividualTerms = x.IndividualTerms.Select(y => new TermListModel.IndividualTerm()
                            {
                                Id = y.Id,
                                BaseTerm = y.BaseTerm,
                                Definition = y.Definition,
                                Romanisation = y.Romanisation,
                                Sentence = y.Sentence,
                                Tags = y.Tags
                            }).ToList()
                    }));

            SearchGridResult<TermListModel> result = new SearchGridResult<TermListModel>()
            {
                Items = termViewList,
                Page = page.Value,
                Sort = sort,
                Direction = sortDir,
                RowsPerPage = perPage ?? 15,
                TotalRows = searchResult.TotalRows
            };

            return PartialView("Partials/_list", result);
        }

        [AjaxRoute]
        public ActionResult PerformAction(string action, Guid[] ids, string input)
        {
            try
            {
                if(ids == null || ids.Length == 0)
                {
                    return new JsonNetResult() { Data = "OK" };
                }

                IList<Term> terms = new List<Term>();

                switch(action)
                {
                    case "add":
                        ids.ToList().ForEach(x => terms.Add(_termService.Find(x, includeIndividualTerms: true)));
                        if(!string.IsNullOrWhiteSpace(input))
                        {
                            foreach(var t in terms)
                            {
                                foreach(var it in t.IndividualTerms)
                                {
                                    it.Tags = TagHelper.ToString(TagHelper.Merge(TagHelper.Split(it.Tags), TagHelper.Split(input)));
                                    t.UpdateIndividualTerm(it.Id, it);
                                }

                                _termService.Save(t);
                            }
                        }
                        break;

                    case "remove":
                        ids.ToList().ForEach(x => terms.Add(_termService.Find(x, includeIndividualTerms: true)));
                        if(!string.IsNullOrWhiteSpace(input))
                        {
                            foreach(var t in terms)
                            {
                                foreach(var it in t.IndividualTerms)
                                {
                                    it.Tags = TagHelper.ToString(TagHelper.Remove(TagHelper.Split(it.Tags), TagHelper.Split(input)));
                                    t.UpdateIndividualTerm(it.Id, it);
                                }

                                _termService.Save(t);
                            }
                        }
                        break;

                    case "known":
                    case "unknown":
                    case "notseen":
                    case "ignored":
                        ids.ToList().ForEach(x => terms.Add(_termService.Find(x)));
                        var newState = (TermState)Enum.Parse(typeof(TermState), action, true);
                        foreach(var t in terms)
                        {
                            t.State = newState;
                            _termService.Save(t);
                        }
                        break;
                }

                return new JsonNetResult() { Data = "OK" };
            }
            catch(Exception e)
            {
                return new JsonNetResult() { Data = "FAIL" };
            }
        }
    }
}
