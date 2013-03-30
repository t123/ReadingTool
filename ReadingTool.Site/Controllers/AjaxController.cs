﻿#region License
// AjaxController.cs is part of ReadingTool.Site
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Common;
using ReadingTool.Common.Extensions;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.Ajax;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class AjaxController : BaseController
    {
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<User> _userRepository;
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Tag> _tagRepository;
        private readonly ITextService _textService;

        public AjaxController(
            Repository<Language> languageRepository,
            Repository<Text> textRepository,
            Repository<User> userRepository,
            Repository<Term> termRepository,
            Repository<Tag> tagRepository,
            ITextService textService
            )
        {
            _languageRepository = languageRepository;
            _textRepository = textRepository;
            _userRepository = userRepository;
            _termRepository = termRepository;
            _tagRepository = tagRepository;
            _textService = textService;
        }

        [AjaxRoute]
        [HttpPost]
        public JsonNetResult ResetTerm(Guid termId)
        {
            var term = _termRepository.FindOne(x => x.TermId == termId && x.User == _userRepository.LoadOne(UserId));

            if(term == null)
            {
                throw new Exception("Term not found");
            }

            term.Box = 1;
            term.State = TermState.NotKnown;
            _termRepository.Save(term);

            return new JsonNetResult()
            {
                Data = new TermModel
                {
                    BasePhrase = term.BasePhrase,
                    Definition = term.Definition,
                    Phrase = term.Phrase.ToLowerInvariant(),
                    Sentence = term.Sentence,
                    State = term.State.ToString(),
                    Tags = Tags.ToString(term.Tags.Select(x => x.TagTerm)),
                    TermId = term.TermId,
                    Message = "Term updated to <strong>" + term.State.ToDescription() + " in box 1</strong>",
                    Box = term.Box,
                    StateClass = Term.TermStateToClass(term.State)
                }
            };
        }

        [AjaxRoute]
        [HttpPost]
        public JsonNetResult SaveTerm(SaveTermModel model)
        {
            if(!ModelState.IsValid)
            {
                return new JsonNetResult()
                {
                    Data = new TermModel
                    {
                        State = "failed",
                        Message = "<strong>WARNING</strong>: Term did not save"
                    }
                };
            }

            Term term = null;
            string message;
            short length = (short)model.Phrase.Trim().Split(' ').Length;

            if(model.TermId == null || model.TermId == Guid.Empty)
            {
                term = new Term()
                    {
                        Created = DateTime.Now,
                        Modified = DateTime.Now,
                        BasePhrase = model.BasePhrase.Trim(),
                        Box = 1,
                        Definition = model.Definition.Trim(),
                        Language = _languageRepository.LoadOne(model.LanguageId),
                        User = _userRepository.LoadOne(UserId),
                        Phrase = model.Phrase.Trim(),
                        Sentence = model.Sentence.Trim(),
                        State = (TermState)Enum.Parse(typeof(TermState), model.State, true),
                        Text = _textRepository.LoadOne(model.TextId),
                        Length = length
                    };

                var stateUpdate = Term.NextReviewDate(term);
                term.State = stateUpdate.Item1;
                term.NextReview = stateUpdate.Item2;

                message = "Term created, state is <strong>" + term.State.ToDescription() + "</strong>";
            }
            else
            {
                term = _termRepository.FindOne(x => x.TermId == model.TermId && x.User == _userRepository.LoadOne(UserId));

                if(term == null)
                {
                    throw new Exception("Invalid term");
                }

                term.BasePhrase = model.BasePhrase.Trim();
                term.Sentence = model.Sentence.Trim();
                term.Definition = model.Definition.Trim();
                term.Length = length;

                var newState = (TermState)Enum.Parse(typeof(TermState), model.State, true);
                bool resetBox = false;
                if(term.State != newState && newState == TermState.NotKnown)
                {
                    resetBox = true;
                    term.Box = 1;
                }

                term.State = newState;
                message = "Term updated, state is <strong>" + term.State.ToDescription() + "</strong>";

                if(resetBox)
                {
                    message += ", reset to <strong>box 1</strong>";
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

                var stateUpdate = Term.NextReviewDate(term);
                term.State = stateUpdate.Item1;
                term.NextReview = stateUpdate.Item2;
            }

            _termRepository.Save(term);

            return new JsonNetResult()
            {
                Data = new TermModel
                {
                    BasePhrase = term.BasePhrase,
                    Definition = term.Definition,
                    Phrase = term.Phrase.ToLowerInvariant(),
                    Sentence = term.Sentence,
                    State = term.State.ToString(),
                    Tags = Tags.ToString(term.Tags.Select(x => x.TagTerm)),
                    TermId = term.TermId,
                    Message = message,
                    Box = term.Box,
                    StateClass = Term.TermStateToClass(term.State),
                    Length = term.Length
                }
            };
        }

        [AjaxRoute]
        [HttpPost]
        public JsonNetResult FindTerm(Guid? termId, string spanTerm, Guid languageId)
        {
            spanTerm = (spanTerm ?? "").Trim();
            Term term = null;

            short length = (short)spanTerm.Split(' ').Length;

            if(termId == null || termId == Guid.Empty)
            {

                term = _termRepository.FindAll(x =>
                                                   x.PhraseLower.Equals(spanTerm.ToLowerInvariant()) &&
                                                   x.User == _userRepository.LoadOne(UserId) &&
                                                   x.Language == _languageRepository.LoadOne(languageId)
                                                   )
                    .FirstOrDefault();
            }
            else
            {
                term = _termRepository.FindOne(x => x.TermId == termId && x.User == _userRepository.LoadOne(UserId));
            }

            if(term == null)
            {
                return new JsonNetResult()
                    {
                        Data = new TermModel
                            {
                                Phrase = spanTerm.Trim(),
                                Length = length,
                                State = TermState.NotKnown.ToString(),
                                Message = "New word, default to <strong>UNKNOWN</strong>"
                            }
                    };
            }

            return new JsonNetResult()
                {
                    Data = new TermModel
                        {
                            BasePhrase = term.BasePhrase,
                            Definition = term.Definition,
                            Phrase = term.Phrase,
                            Sentence = term.Sentence,
                            State = term.State.ToString(),
                            Tags = Tags.ToString(term.Tags.Select(x => x.TagTerm)),
                            Box = term.Box,
                            TermId = term.TermId,
                            Message = "Current box : <strong>" + term.Box + "</strong>",
                            Length = term.Length
                        }
                };
        }

        [HttpPost]
        [AjaxRoute]
        public JsonNetResult MarkRemaingAsKnown(Guid languageId, Guid textId, string[] terms)
        {
            if(terms == null || terms.Length == 0)
            {
                return new JsonNetResult() { Data = "OK" };
            }

            try
            {
                var language = _languageRepository.FindOne(languageId);
                var user = _userRepository.LoadOne(UserId);
                var text = _textRepository.LoadOne(textId);

                Regex regex = new Regex(@"([" + language.Settings.RegexWordCharacters + @"])", RegexOptions.Compiled);

                foreach(var term in terms.Distinct(StringComparer.Create(CultureInfo.InvariantCulture, true)))
                {
                    if(string.IsNullOrEmpty(term))
                    {
                        continue;
                    }

                    if(!regex.IsMatch(term))
                    {
                        continue;
                    }

                    _termRepository.Save(new Term()
                    {
                        Created = DateTime.Now,
                        Modified = DateTime.Now,
                        Box = 1,
                        Language = language,
                        User = user,
                        Phrase = term,
                        State = TermState.Known,
                        Text = text,
                    });
                }
            }
            catch(Exception e)
            {
                return new JsonNetResult() { Data = e.Message };
            }

            return new JsonNetResult() { Data = "OK" };
        }

        [AjaxRoute]
        [HttpPost]
        public JsonResult EncodeTerm(Guid languageId, Guid dictionaryId, string input)
        {
            try
            {
                var language = _languageRepository.FindOne(languageId);

                if(language == null)
                {
                    throw new Exception("Language is null");
                }

                var dictionary = language.Dictionaries.FirstOrDefault(x => x.DictionaryId == dictionaryId);

                if(dictionary == null)
                {
                    throw new Exception("Dictionary is null");
                }

                var encoder = Encoding.GetEncoding(dictionary.Encoding);
                string encoded = HttpUtility.UrlEncode(input, encoder);
                string result = dictionary.Url.Replace("###", encoded);

                return new JsonNetResult() { Data = result };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult() { Data = "" };
            }
        }
    }
}
