#region License
// WordsController.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using AutoMapper;
using Ionic.Zip;
using MongoDB.Bson;
using MvcContrib;
using Newtonsoft.Json;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.CsvBuilder;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.LWT;
using ReadingTool.Models.Create.Word;
using ReadingTool.Models.View.Language;
using ReadingTool.Models.View.Word;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class WordsController : BaseController
    {
        private readonly ILanguageService _languageService;
        private readonly IWordService _wordService;
        private readonly IUserService _userService;

        public WordsController(ILanguageService languageService, IWordService wordService, IUserService userService)
        {
            _languageService = languageService;
            _wordService = wordService;
            _userService = userService;
        }

        public ActionResult Index()
        {
            ViewBag.Languages = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageSimpleViewModel>>(_languageService.FindAllForOwner());
            ViewBag.Tags = _wordService.AutocompleteTags("", 100);

            return View();
        }

        [HttpGet]
        public ActionResult Review()
        {
            var model = TempData["reviewWordsModel"] == null ? new ReviewWordsModel() : TempData["reviewWordsModel"] as ReviewWordsModel;
            ViewBag.Languages = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageSimpleViewModel>>(_languageService.FindAllForOwner());

            if(model.LanguageId != ObjectId.Empty)
            {
                ViewBag.Words =
                    Mapper.Map<IEnumerable<Word>, IEnumerable<WordViewModel>>(
                        _wordService.FindAllForReview(model.LanguageId, model.NumberOfWords));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(ReviewWordsModel model)
        {
            ViewBag.Languages = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageSimpleViewModel>>(_languageService.FindAllForOwner());

            if(ModelState.IsValid)
            {
                ViewBag.Words =
                    Mapper.Map<IEnumerable<Word>, IEnumerable<WordViewModel>>(
                        _wordService.FindAllForReview(model.LanguageId, model.NumberOfWords));
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult SaveReviewWords()
        {
            throw new NotSupportedException();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveReviewWords(ReviewWordsModel model)
        {
            TempData["reviewWordsModel"] = model;
            Dictionary<ObjectId, string> updates = new Dictionary<ObjectId, string>();

            foreach(var key in Request.Form.AllKeys)
            {
                ObjectId objectId;
                if(ObjectId.TryParse(key, out objectId))
                {
                    updates.Add(objectId, Request.Form[key]);
                }
            }
            _wordService.SaveReviews(updates);

            return this.RedirectToAction(x => x.Review());
        }

        [HttpGet]
        [AutoMap(typeof(Word), typeof(WordModel))]
        public ActionResult Edit(string id)
        {
            var word = _wordService.FindOne(id);

            if(word == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Word not foud");
            }

            ViewBag.Languages = _languageService.FindAllForOwner().ToDictionary(x => x.LanguageId, x => x.Name);

            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(string id, WordModel model)
        {
            if(ModelState.IsValid)
            {
                var word = _wordService.FindOne(id);
                TryUpdateModel(word, new[]
                                         {
                                             "LanguageId", "State", "WordPhrase", "BaseWord", "Sentence",
                                             "Definition", "Romanisation"
                                         });
                word.Tags = TagHelper.Split(model.Tags);
                _wordService.Save(word);
                return this.RedirectToAction(x => x.Edit(id));
            }

            ViewBag.Languages = _languageService.FindAllForOwner().ToDictionary(x => x.LanguageId, x => x.Name);

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ImportLwt()
        {
            return this.RedirectToAction(x => x.Import());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportLwt(LwtWordsImportModel model)
        {
            StringBuilder sb = new StringBuilder();

            if(ModelState.IsValid)
            {
                string data = string.Empty;
                var languages = _languageService.FindAllForOwner().ToDictionary(x => x.Name);

                using(TextReader tr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                {
                    data = tr.ReadToEnd();
                }

                IList<LwtWordModel> words = new List<LwtWordModel>();
                ObjectId? found = null;
                string languageName = string.Empty;
                string languageColor = string.Empty;
                ObjectId languageSId = ObjectId.Empty;

                foreach(var line in data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {

                    try
                    {
                        var pieces = line.Split(new[] { '\t' }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

                        if(!languages.ContainsKey(pieces[5]))
                        {
                            sb.AppendFormat("The language <strong>{0}</strong> is not defined\n", pieces[5]);
                            continue;
                        }
                        Language l = languages.GetValueOrDefault(pieces[5], value => Language.Unknown());
                        var languageId = l.LanguageId;
                        languageName = l.Name;
                        languageColor = l.Colour;
                        languageSId = l.SystemLanguageId;

                        if(found == null)
                        {
                            found = languageId;
                        }
                        else
                        {
                            if(found.Value != languageId)
                            {
                                sb.AppendFormat("Your file contains multiple languages\n");
                                break;
                            }
                        }

                        LwtWordModel word = new LwtWordModel()
                                                {
                                                    Term = pieces[0],
                                                    Translation = pieces[1] == "*" ? string.Empty : pieces[1],
                                                    Sentence = pieces[2],
                                                    Romanisation = pieces[3],
                                                    Box = int.Parse(pieces[4]),
                                                    LanguageId = languageId,
                                                    Tags = TagHelper.Split(pieces[7])
                                                };

                        words.Add(word);
                    }
                    catch(Exception e)
                    {
                        sb.AppendFormat("The following line is invalid: <pre>{0}</pre>\n", line);
                    }
                }

                if(sb.Length == 0)
                {
                    var currentWords = _wordService.FindAllForOwner();

                    Hashtable oldStyle = new Hashtable(); //Pesky duplicate keys!
                    foreach(var word in currentWords) oldStyle[word.WordPhraseLower] = word.WordId;

                    foreach(var word in words)
                    {
                        //if(oldStyle[word.Term.ToLowerInvariant()] != null)
                        //{
                        //    sb.AppendFormat("The word <strong>{0}</strong> was skipped\n", word.Term);
                        //}
                        //else
                        {
                            Word actualWord = new Word()
                                                  {
                                                      WordPhrase = word.Term,
                                                      Tags = word.Tags,
                                                      Box = word.Box,
                                                      State = word.WordState,
                                                      Sentence = word.Sentence,
                                                      Romanisation = word.Romanisation,
                                                      Definition = word.Translation,
                                                      LanguageId = word.LanguageId,
                                                      LanguageColour = languageColor,
                                                      LanguageName = languageName,
                                                      SystemLanguageId = languageSId,
                                                      BaseWord = ""
                                                  };

                            _wordService.Save(actualWord);
                        }
                    }

                    if(sb.Length == 0)
                    {
                        return this.RedirectToAction(x => x.Import()).Success("Your words have been imported");
                    }
                }
            }
            else
            {
                sb.AppendLine("Please supply a file");
            }

            return View((object)sb.ToString());
        }

        [HttpGet]
        public ActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Export(string format)
        {
            if(!string.IsNullOrEmpty(format))
            {
                var words = _wordService.FindAllForOwner();

                string asText = string.Empty;
                string filename = string.Empty;

                switch(format)
                {
                    case "JSON":
                        var jsonWords = words.Select(x =>
                                                                  new
                                                                      {
                                                                          WordId = x.WordId.ToString(),
                                                                          Language = x.LanguageName,
                                                                          Word = x.WordPhrase,
                                                                          Sentence = x.Sentence,
                                                                          BaseWord = x.BaseWord,
                                                                          Definition = x.Definition,
                                                                          Romanisation = x.Romanisation,
                                                                          Box = x.Box,
                                                                          State = x.State.ToString(),
                                                                          Created = x.Created.ToString("o"),
                                                                          Modified = x.Modified.ToString("o"),
                                                                          NextReview = x.NextReview.ToString("o"),
                                                                          Tags = x.Tags,
                                                                      }
                    );
                        asText = JsonConvert.SerializeObject(jsonWords, Formatting.Indented);
                        filename = "words.json";
                        break;

                    case "XML":
                        XDocument doc = new XDocument();
                        XElement root = new XElement("words");

                        foreach(var x in words)
                        {
                            XElement element = new XElement("word",
                                new XElement("WordId", x.WordId),
                                new XElement("Language", x.LanguageName),
                                new XElement("Word", x.WordPhrase),
                                new XElement("Sentence", x.Sentence),
                                new XElement("BaseWord", x.BaseWord),
                                new XElement("Definition", x.Definition),
                                new XElement("Romanisation", x.Romanisation),
                                new XElement("Box", x.Box),
                                new XElement("State", x.State.ToString()),
                                new XElement("Created", x.Created.ToString("o")),
                                new XElement("Modified", x.Modified.ToString("o")),
                                new XElement("NextReview", x.NextReview.ToString("o")),
                                new XElement("Tags", new XElement("tag", x.Tags.Select(y => y)))
                                );

                            root.Add(element);
                        }

                        doc.Add(root);
                        asText = doc.ToString();
                        filename = "words.xml";
                        break;

                    case "CSV":
                    case "TSV":
                        CsvBuilder csv = format == "CSV" ? new CsvBuilder() : new CsvBuilder(CsvType.TSV, false);
                        csv.AddHeader(new[]
                                  {
                                      "WordId", "Language", "Created", "Modified", "Word", "State", "Definition", "Base Word",
                                      "Romanisation", "Tags", "Box", "Recognition Sentence", "Production Sentence"
                                  });

                        foreach(var word in words)
                        {
                            csv.AddRow(new[]
                               {
                                   word.WordId.ToString(),
                                   word.LanguageName,
                                   word.Created.ToString("o"),
                                   word.Modified.ToString("o"),
                                   word.WordPhrase,
                                   word.State.ToString(),
                                   word.Definition,
                                   word.BaseWord,
                                   word.Romanisation,
                                   string.Join(",", word.Tags),
                                   word.Box.ToString(),
                                   word.Sentence,
                                   word.Sentence.ReplaceString(word.WordPhrase, "[...]", StringComparison.InvariantCultureIgnoreCase)
                               });
                        }

                        asText = csv.ToString();
                        filename = "words." + csv.CsvType.ToString().ToLowerInvariant();
                        break;

                    default:
                        break;
                }

                MemoryStream ms = new MemoryStream();
                using(ZipFile zip = new ZipFile())
                {
                    zip.AddEntry(filename, asText, Encoding.UTF8);
                    zip.Save(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/zip", "words.zip");
            }

            return this.RedirectToAction(x => x.Export()).Error("Please choose a format");
        }

        [AutoMap(typeof(IEnumerable<Word>), typeof(IEnumerable<WordShareModel>))]
        public ActionResult Shared(string word, string languageId)
        {
            var user = _userService.FindOne(UserId);

            if(user == null)
            {
                return this.RedirectToAction<RegistrationController>(x => x.SignIn());
            }

            if(string.IsNullOrEmpty(word))
            {
                return View(new List<WordShareModel>()).Error("No word was specified");
            }

            if(string.IsNullOrEmpty(languageId))
            {
                return View(new List<WordShareModel>()).Error("No language was specified");
            }

            ObjectId id;
            if(!ObjectId.TryParse(languageId, out id))
            {
                return View(new List<WordShareModel>()).Error("The requested language is invalid");
            }

            var language = _languageService.FindOne(id);
            var words = _wordService.FindSharedDefinitions(user, word, language.SystemLanguageId).ToArray();

            foreach(var item in words)
            {
                string sentence = Server.HtmlEncode(item.Sentence);
                string encodedWord = Server.HtmlEncode(item.WordPhrase);

                if(!string.IsNullOrEmpty(item.Definition))
                {
                    item.Sentence = sentence.Replace(encodedWord, string.Format(@"<a class=""s"" title=""{0}"">{1}</a>", Server.HtmlEncode(item.Definition), encodedWord));
                }
                else
                {
                    item.Sentence = sentence.Replace(encodedWord, string.Format(@"<strong><u>{0}</u></strong>", encodedWord));
                }
            }

            return View(words);
        }
    }
}
