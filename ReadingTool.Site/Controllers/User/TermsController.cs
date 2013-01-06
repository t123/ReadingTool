using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ionic.Zip;
using Ionic.Zlib;
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

        [AjaxRoute]
        public ActionResult ExportSelected(string ids)
        {
            var list = new List<Term>();

            if(!string.IsNullOrEmpty(ids))
            {
                foreach(var id in ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Guid i;

                    if(!Guid.TryParse(id, out i))
                        continue;

                    var term = _termService.Find(i, true);
                    if(term == null)
                    {
                        continue;
                    }

                    list.Add(term);
                }
            }

            return CreateExportFile(list);
        }

        [AjaxRoute]
        public ActionResult ExportTerms(string filter)
        {
            var result = _termService.FilterTerms(new SearchOptions()
                {
                    Filter = filter,
                    Sort = "language",
                    IgnorePaging = true
                });

            foreach(var t in result.Results)
            {
                t.AddIndividualTerms(_termService.FindIndividualTerms(t));
            }

            return CreateExportFile(result.Results);
        }

        private ActionResult CreateExportFile(IEnumerable<Term> terms)
        {
            var languages = _languageService.FindAll().ToDictionary(x => x.Id, x => x.Name);

            StringBuilder csvFile = new StringBuilder();
            foreach(var term in terms)
            {
                if(term.IndividualTerms.Any())
                {
                    foreach(var it in term.IndividualTerms)
                    {
                        var export = new TermExportModel()
                            {
                                BaseTerm = it.BaseTerm,
                                Box = term.Box,
                                Definition = it.Definition.Replace("\n", "<br/>").Replace("\r", ""),
                                Id = term.Id,
                                IndividualTermId = it.Id,
                                LanguageId = it.LanguageId,
                                LanguageName = languages.GetValueOrDefault(it.LanguageId, "Unknown"),
                                NextReview = term.NextReview,
                                Romanisation = it.Romanisation,
                                Sentence = it.Sentence.ReplaceString(it.BaseTerm, "<strong>" + it.BaseTerm + "</strong>", StringComparison.InvariantCultureIgnoreCase),
                                State = term.State.ToDescription(),
                                Tags = it.Tags,
                                TermPhrase = term.TermPhrase
                            };

                        csvFile.AppendLine(export.ToString());
                    }
                }
                else
                {
                    var export = new TermExportModel()
                    {
                        BaseTerm = "",
                        Box = term.Box,
                        Definition = "",
                        Id = term.Id,
                        IndividualTermId = null,
                        LanguageId = term.LanguageId,
                        LanguageName = languages.GetValueOrDefault(term.LanguageId, "Unknown"),
                        NextReview = term.NextReview,
                        Romanisation = "",
                        Sentence = "",
                        State = term.State.ToDescription(),
                        Tags = "",
                        TermPhrase = term.TermPhrase
                    };

                    csvFile.AppendLine(export.ToString());
                }
            }

            if(csvFile.Length > 1)
            {
                csvFile.Remove(csvFile.Length - 2, 2);
            }

            if(csvFile.Length < 100 * 1024)
            {
                return new FileContentResult(Encoding.UTF8.GetBytes(csvFile.ToString()), "text/csv") { FileDownloadName = "terms.tsv" };
            }
            else
            {
                MemoryStream ms = new MemoryStream();
                using(ZipFile zip = new ZipFile())
                {
                    zip.CompressionMethod = CompressionMethod.BZip2;
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.AddEntry("terms.tsv", csvFile.ToString(), Encoding.UTF8);
                    zip.Save(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/zip", "terms.zip");
            }
        }
    }
}
