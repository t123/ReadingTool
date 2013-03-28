using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Ionic.Zip;
using Newtonsoft.Json;
using ReadingTool.Common.Search;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Repository;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Languages;
using ReadingTool.Site.Models.Texts;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class TextsController : Controller
    {
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<User> _userRepository;
        private readonly Repository<Term> _termRepository;
        private readonly ITextService _textService;
        private readonly IParserService _parserService;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private long UserId
        {
            get { return long.Parse(HttpContext.User.Identity.Name); }
        }

        public TextsController(
            Repository<Language> languageRepository,
            Repository<Text> textRepository,
            Repository<User> userRepository,
            Repository<Term> termRepository,
            ITextService textService,
            IParserService parserService
            )
        {
            _languageRepository = languageRepository;
            _textRepository = textRepository;
            _userRepository = userRepository;
            _termRepository = termRepository;
            _textService = textService;
            _parserService = parserService;
        }

        public ActionResult Index()
        {
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

            var texts = _textRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId));
            var languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).ToDictionary(x => x.Name.ToLowerInvariant(), x => x.LanguageId);

            var filterTerms = SearchFilterParser.Parse(filter);

            foreach(var term in filterTerms.Other)
            {
                if(languages.ContainsKey(term))
                {
                    texts = texts.Where(x => x.Language1.LanguageId == languages[term]);
                }
                else
                {
                    texts = texts.Where(x => x.Title.Contains(term) || x.CollectionName.Contains(term));
                }
            }

            var count = texts.Count();

            switch(so.Sort)
            {
                case "collectionname":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    break;

                case "lastread":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.LastRead);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.LastRead);
                    }
                    break;

                case "title":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.Title);
                    }
                    break;

                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.Language1.Name).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.Language1.Name).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    break;
            }

            texts = texts.Skip(so.Skip).Take(so.RowsPerPage);

            var searchResult = new SearchResult<TextViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Text>, IEnumerable<TextViewModel>>(texts),
                TotalRows = count
            };

            var result = new SearchGridResult<TextViewModel>()
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
        public ActionResult Add()
        {
            return View(new TextModel() { LanguageList = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name) });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Add(TextModel model)
        {
            if(!string.IsNullOrEmpty(model.L2Text) && model.Language2Id == null)
            {
                ModelState.AddModelError("Language2Id", "Please select a language");
            }

            if(!ModelState.IsValid)
            {
                model.LanguageList = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name);
                return View(model);
            }

            Text text = new Text()
                {
                    CollectionName = model.CollectionName,
                    CollectionNo = model.CollectionNo,
                    Language1 = _languageRepository.LoadOne(model.Language1Id),
                    Language2 = model.Language2Id.HasValue ? _languageRepository.LoadOne(model.Language2Id) : null,
                    User = _userRepository.LoadOne(UserId),
                    Title = model.Title,
                    L1Text = model.L1Text,
                    L2Text = model.L2Text,
                    AudioUrl = model.AudioUrl
                };

            _textService.Save(text);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            var text = _textRepository.FindOne(x => x.TextId == id && x.User.UserId == UserId);

            if(text == null)
            {
                return RedirectToAction("Index");
            }

            var model = new TextModel
                {
                    LanguageList = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name),
                    CollectionName = text.CollectionName,
                    CollectionNo = text.CollectionNo,
                    Language1Id = text.Language1.LanguageId,
                    Language2Id = text.Language2 == null ? (long?)null : text.Language2.LanguageId,
                    TextId = text.TextId,
                    Title = text.Title,
                    AudioUrl = text.AudioUrl,
                    L1Text = text.L1Text,
                    L2Text = text.L2Text
                };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(long id, TextModel model)
        {
            if(!string.IsNullOrEmpty(model.L2Text) && model.Language2Id == null)
            {
                ModelState.AddModelError("Language2Id", "Please select a language");
            }

            if(!ModelState.IsValid)
            {
                model.LanguageList = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).OrderBy(x => x.Name).ToDictionary(x => x.LanguageId, x => x.Name);
                return View(model);
            }

            var text = _textRepository.FindOne(x => x.TextId == id && x.User.UserId == UserId);

            if(text == null || id != model.TextId)
            {
                return RedirectToAction("Index");
            }

            text.CollectionName = model.CollectionName;
            text.CollectionNo = model.CollectionNo;
            text.Language1 = _languageRepository.LoadOne(model.Language1Id);
            text.Language2 = model.Language2Id == null ? null : _languageRepository.LoadOne(model.Language2Id);
            text.Title = model.Title;
            text.L1Text = model.L1Text;
            text.L2Text = model.L2Text;
            text.AudioUrl = model.AudioUrl;

            _textService.Save(text);

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            _textService.Delete(id);

            return RedirectToAction("Index");
        }

        public ActionResult Read(long id)
        {
            var text = _textService.FindOne(id);

            if(text == null)
            {
                return RedirectToAction("Index");
            }

            return View(Create(text, false));
        }

        public ActionResult ReadParallel(long id)
        {
            var text = _textService.FindOne(id);

            if(text == null)
            {
                return RedirectToAction("Index");
            }

            return View("Read", Create(text, true));
        }

        public FileContentResult DownloadLatex(long id)
        {
            var text = _textService.FindOne(id);

            if(text == null)
            {
                throw new FileNotFoundException();
            }

            var terms = _termRepository.FindAll(x => x.Language == text.Language1 && x.User == _userRepository.LoadOne(UserId)).ToArray();
            var latexParser = DependencyResolver.Current.GetService<LatexParserService>();
            var parsed = latexParser.Parse(false, text.Language1, text.Language2, terms, text);

            return new FileContentResult(Encoding.UTF8.GetBytes(parsed), "application/x-latex")
                {
                    FileDownloadName = Path.Combine(text.Title, ".tex")
                };
        }

        private ReadModel Create(Text text, bool asParallel)
        {
            if(asParallel && text.Language2 == null)
            {
                asParallel = false;
            }

            var terms = _termRepository.FindAll(x => x.Language == text.Language1 && x.User == _userRepository.LoadOne(UserId)).ToArray();
            var parsed = _parserService.Parse(asParallel, text.Language1, text.Language2, terms, text);

            var nextText = _textRepository.FindAll(x => x.User == text.User && x.Language1 == text.Language1 && x.CollectionName == text.CollectionName && x.CollectionNo > text.CollectionNo).OrderBy(x => x.CollectionNo).FirstOrDefault();
            var previousText = _textRepository.FindAll(x => x.User == text.User && x.Language1 == text.Language1 && x.CollectionName == text.CollectionName && x.CollectionNo < text.CollectionNo).OrderByDescending(x => x.CollectionNo).FirstOrDefault();

            text.LastRead = DateTime.Now;
            _textRepository.Save(text);

            return new ReadModel()
                {
                    AsParallel = asParallel,
                    ParsedText = parsed,
                    Text = Mapper.Map<Text, TextViewModel>(text),
                    Language = Mapper.Map<Language, LanguageViewModel>(text.Language1),
                    Language2 = !asParallel || text.Language2 == null ? null : Mapper.Map<Language, LanguageViewModel>(text.Language2),
                    User = Mapper.Map<User, AccountModel.UserModel>(text.User),
                    PagedTexts = new Tuple<long?, long?>(previousText == null ? (long?)null : previousText.TextId, nextText == null ? (long?)null : nextText.TextId)
                };
        }


        [HttpGet]
        public ActionResult Import()
        {
            var languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).Select(x => x.Name).OrderBy(x => x).ToArray();
            ViewBag.Languages = languages;
            ViewBag.SampleJson = TempData["SampleJson"] ?? "";
            ViewBag.SampleJsonModel = TempData["SampleJsonModel"] ?? null;

            return View();
        }

        public ActionResult CreateSample(JsonSampleModel model)
        {
            if(ModelState.IsValid)
            {
                TextImport ti = new TextImport()
                {
                    Defaults = new TextImport.JsonDefaults()
                    {
                        AutoNumberCollection = model.AutoNumberCollection,
                        CollectionName = model.CollectionName,
                        L1LanguageName = model.L1Name,
                        L2LanguageName = model.L2Name,
                        StartCollectionWithNumber = model.StartCollectionWithNumber,
                    },
                };

                ti.Items = new TextImport.JsonTextItem[model.NumberOfItems ?? 1];
                for(int i = 0; i < (model.NumberOfItems ?? 1); i++)
                {
                    ti.Items[i] = new TextImport.JsonTextItem()
                    {
                        L1LanguageName = "",
                        L2LanguageName = "",
                        Title = "",
                        AudioUrl = "",
                        CollectionName = "",
                        L1Text = "",
                        L2Text = "",
                    };
                };

                TempData["SampleJson"] = JsonConvert.SerializeObject(ti, Formatting.Indented);
                this.FlashSuccess("Your sample is below. Please make sure your editor has UTF-8 encoding.");
            }
            else
            {
                this.FlashError(Messages.FORM_FAIL);
            }

            TempData["SampleJsonModel"] = model;
            return RedirectToAction("Import");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(TextImportModel model)
        {
            ViewBag.Languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId)).Select(x => x.Name).OrderBy(x => x).ToArray();

            if(ModelState.IsValid && model.File != null && model.File.ContentLength > 0)
            {
                try
                {
                    TextImport json;

                    if(ZipFile.IsZipFile(model.File.InputStream, false))
                    {
                        model.File.InputStream.Position = 0;
                        using(var zip = ZipFile.Read(model.File.InputStream))
                        {
                            var data = zip[0];

                            if(data == null)
                            {
                                throw new Exception("There is no file in the ZIP archive");
                            }

                            using(var sr = new StreamReader(data.OpenReader()))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                json = serializer.Deserialize<TextImport>(new JsonTextReader(sr));

                                if(json == null) throw new Exception("File is empty");
                                if(json.Items == null || json.Items.Length == 0) throw new Exception("No texts are specified");
                            }
                        }
                    }
                    else
                    {
                        using(var sr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            json = serializer.Deserialize<TextImport>(new JsonTextReader(sr));

                            if(json == null) throw new Exception("File is empty");
                            if(json.Items == null || json.Items.Length == 0) throw new Exception("No texts are specified");
                        }
                    }

                    int imported = _textService.Import(json);

                    this.FlashSuccess("{0} texts were imported", imported);
                    return this.RedirectToAction("Import");
                }
                catch(Exception e)
                {
                    this.FlashError(e.Message);
                }

                return View(model);
            }

            this.FlashError(Messages.FORM_FAIL);
            return View(model);
        }
    }
}
