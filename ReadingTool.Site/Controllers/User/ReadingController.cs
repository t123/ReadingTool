using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ServiceStack;

namespace ReadingTool.Site.Controllers.User
{
    [ValidateInput(false)]
    public class ReadingController : Controller
    {
        private readonly ITextService _textService;
        private readonly ITermService _termService;
        private readonly ILanguageService _languageService;
        private readonly IUserIdentity _identity;

        public const string OK = "OK";
        public const string FAIL = "FAIL";

        public class ResponseMessage
        {
            public string Result { get; private set; }
            public string Message { get; set; }
            public object Data { get; set; }

            public ResponseMessage(string result)
            {
                Result = result;
            }
        }

        public ReadingController(
            ITextService textService,
            ITermService termService,
            ILanguageService languageService,
            IPrincipal principal)
        {
            _textService = textService;
            _termService = termService;
            _languageService = languageService;
            _identity = principal.Identity as IUserIdentity;
        }

        [HttpPost]
        public ActionResult Index()
        {
            throw new NotSupportedException();
        }

        [AjaxRoute]
        public JsonResult SaveAudioLength(Guid textId, double? length)
        {
            try
            {
                var text = _textService.Find(textId);

                if(text == null)
                {
                    throw new Exception("Text not found");
                }

                text.AudioLength = length.HasValue ? (int?)Math.Ceiling(length.Value) : null;
                _textService.Save(text, ignoreModificationTime: true);

                return new JsonNetResult() { Data = new ResponseMessage(OK) };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

        [AjaxRoute]
        public JsonResult ChangeRead(Guid textId, int direction, int words)
        {
            try
            {
                var text = _textService.Find(textId);

                if(text == null)
                {
                    throw new Exception("Text not found");
                }

                if(direction > 0)
                {
                    text.TimesRead = (text.TimesRead ?? 0) + 1;
                    text.WordsRead = (text.WordsRead ?? 0) + words;
                }
                else
                {
                    text.TimesRead = (text.TimesRead ?? 0) - 1;
                    text.WordsRead = (text.WordsRead ?? 0) - words;
                }

                if(text.TimesRead < 0) text.TimesRead = 0;
                if(text.WordsRead < 0) text.WordsRead = 0;

                _textService.Save(text, ignoreModificationTime: true);

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = string.Format("{0} ({1} words)",
                            text.TimesRead,
                            text.WordsRead
                            )
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

        [AjaxRoute]
        public JsonResult ChangeListened(Guid textId, int direction)
        {
            try
            {
                var text = _textService.Find(textId);

                if(text == null)
                {
                    throw new Exception("Text not found");
                }

                if(direction > 0)
                {
                    text.TimesListened = (text.TimesListened ?? 0) + 1;
                    text.ListenLength = (text.ListenLength ?? 0) + text.AudioLength;
                }
                else
                {
                    text.TimesListened = (text.TimesListened ?? 0) - 1;
                    text.ListenLength = (text.ListenLength ?? 0) - text.AudioLength;
                }

                if(text.TimesListened < 0) text.TimesListened = 0;
                if(text.ListenLength < 0) text.ListenLength = 0;
                _textService.Save(text, ignoreModificationTime: true);

                return new JsonNetResult()
                    {
                        Data = new ResponseMessage(OK)
                            {
                                Message = string.Format("{0} ({1})",
                                    text.TimesListened,
                                    DateFormatter.SecondsToHourMinuteSeconds(text.ListenLength)
                                    )
                            }
                    };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

        [AjaxRoute]
        public JsonResult EncodeTerm(Guid languageId, Guid dictionaryId, string input)
        {
            try
            {
                var language = _languageService.Find(languageId);

                if(language == null)
                {
                    throw new Exception("Language is null");
                }

                var dictionary = language.Dictionaries.FirstOrDefault(x => x.Id == dictionaryId);

                if(dictionary == null)
                {
                    throw new Exception("Dictionary is null");
                }

                var encoder = Encoding.GetEncoding(dictionary.UrlEncoding);
                string encoded = HttpUtility.UrlEncode(input, encoder);
                string result = dictionary.Url.Replace("###", encoded);

                return new JsonNetResult() { Data = new ResponseMessage(OK) { Message = result } };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

        internal class JsonIndividualTermResult
        {
            public Guid Id { get; set; }
            public string BaseTerm { get; set; }
            public string Definition { get; set; }
            public string Romanisation { get; set; }
            public string Tags { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string Sentence { get; set; }

            public JsonIndividualTermResult(IndividualTerm i)
            {
                Id = i.Id;
                BaseTerm = i.BaseTerm;
                Definition = i.Definition;
                Romanisation = i.Romanisation;
                Tags = i.Tags;
                Sentence = i.Sentence;

                if(Id == Guid.Empty)
                {
                    Title = "New definition";
                    Message = "new definition";
                }
                else
                {
                    Message = string.Format("last changed {0} ago", i.Modified.ToHumanAgo());
                    Title = i.BaseTerm;
                }
            }
        }

        internal class JsonTermResult
        {
            public Guid Id { get; set; }
            public int Length { get; set; }
            public string StateClass { get; set; }
            public string Box { get; set; }
            public string Message { get; set; }
            public string Definition { get; set; }

            public JsonIndividualTermResult[] IndividualTerms { get; set; }

            public JsonTermResult(Term term)
            {
                Id = term.Id;
                Length = term.Length;
                StateClass = Constants.TermStatesToClass[term.State];
                Box = term.Box.HasValue ? term.Box.Value.ToString() : "new";
                Message = term.NextReview.HasValue
                              ? "review due in " + (term.NextReview.Value - DateTime.Now).ToHumanAgo()
                              : "no review due";

                Definition = term.Definition;
                IndividualTerms = term.IndividualTerms.Select(t => new JsonIndividualTermResult(t)).ToArray();
            }
        }

        [AjaxRoute]
        public JsonResult FindTerm(Guid languageId, string termPhrase)
        {
            try
            {
                var term = _termService.Find(languageId, termPhrase);

                if(term == null)
                {
                    term = new Term()
                        {
                            State = TermState.Unknown,
                        };
                }

                term.AddIndividualTerm(new IndividualTerm() { Id = Guid.Empty, Created = DateTime.Now.AddYears(1) }, true);

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Could not find word",
                        Data = new JsonTermResult(term)
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Could not find word",
                    }
                };
            }
        }

        private Term NewTerm(Guid languageId, string termPhrase, TermState state = TermState.Unknown)
        {

            var term = new Term()
                {
                    Id = Guid.Empty,
                    LanguageId = languageId,
                    Length = 1,
                    State = state,
                    TermPhrase = termPhrase
                };

            return BoxAndReviewReset(term);
        }

        private Term BoxAndReviewReset(Term term)
        {
            var langauge = _languageService.Find(term.LanguageId);
            var review = langauge.Review ?? Review.Default;
            term.Box = 1;
            term.NextReview = DateTime.Now.AddMinutes(review.Box1Minutes ?? Review.Default.Box1Minutes.Value);
            return term;
        }

        [AjaxRoute]
        public JsonResult Quicksave(Guid languageId, string termPhrase)
        {
            try
            {
                Term term = _termService.Find(languageId, termPhrase);

                if(term == null)
                {
                    term = NewTerm(languageId, termPhrase);
                }
                else
                {
                    switch(term.State)
                    {
                        case TermState.NotSeen:
                            BoxAndReviewReset(term);
                            term.State = TermState.Unknown;
                            break;

                        case TermState.Unknown:
                            term.State = TermState.Known;
                            break;

                        case TermState.Known:
                            term.State = TermState.NotSeen;
                            break;
                    }
                }

                _termService.Save(term);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = "Saved, state is " + term.State.ToDescription().ToUpperInvariant(),
                        Data = new
                        {
                            termPhrase = term.TermPhrase.ToLowerInvariant(),
                            term = new JsonTermResult(term)
                        }
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                    {
                        Data = new ResponseMessage(FAIL)
                            {
                                Message = "Save failed"
                            }
                    };
            }
        }

        public class JsonSaveTerm
        {
            public Guid Id { get; set; }
            public string BaseTerm { get; set; }
            public string Romanisation { get; set; }
            public string Definition { get; set; }
            public string Tags { get; set; }
            public string Sentence { get; set; }
        }

        [AjaxRoute]
        public JsonResult SaveTerm(Guid languageId, Guid textId, Guid termId, string state, string termPhrase, JsonSaveTerm[] model)
        {
            try
            {
                Term term = _termService.Find(languageId, termPhrase);

                if(term == null)
                {
                    term = NewTerm(languageId, termPhrase, Constants.ClassToTermStates[state]);
                }
                else
                {
                    var newState = Constants.ClassToTermStates[state];

                    if(term.State != TermState.Unknown && newState == TermState.Unknown)
                    {
                        term = BoxAndReviewReset(term);
                    }

                    term.State = newState;
                }

                foreach(var it in model ?? new JsonSaveTerm[] { })
                {
                    if(it.Id == Guid.Empty)
                    {
                        term.AddIndividualTerm(new IndividualTerm()
                            {
                                BaseTerm = it.BaseTerm,
                                Definition = it.Definition,
                                Romanisation = it.Romanisation,
                                Sentence = it.Sentence,
                                Tags = it.Tags,
                                TextId = textId,
                                LanguageId = languageId
                            });
                    }
                    else
                    {
                        term.UpdateIndividualTerm(it.Id, new IndividualTerm()
                            {
                                BaseTerm = it.BaseTerm,
                                Definition = it.Definition,
                                Romanisation = it.Romanisation,
                                Sentence = it.Sentence,
                                Tags = it.Tags,
                                LanguageId = languageId
                            });
                    }
                }

                _termService.Save(term);

                Term newTerm = _termService.Find(languageId, termPhrase);
                newTerm.AddIndividualTerm(new IndividualTerm() { Id = Guid.Empty, Created = DateTime.Now.AddYears(1) }, true);

                return new JsonNetResult()
                    {
                        Data = new ResponseMessage(OK)
                            {
                                Message = "Saved, state is " + term.State.ToDescription().ToUpperInvariant(),
                                Data = new
                                    {
                                        termPhrase = term.TermPhrase.ToLowerInvariant(),
                                        term = new JsonTermResult(newTerm)
                                    }
                            }
                    };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Save failed"
                    }
                };
            }
        }

        [AjaxRoute]
        public JsonResult ResetTerm(Guid languageId, string termPhrase)
        {
            try
            {
                Term term = _termService.Find(languageId, termPhrase);

                if(term == null)
                {
                    return new JsonNetResult()
                    {
                        Data = new ResponseMessage(FAIL)
                        {
                            Message = "Cannot reset new term"
                        }
                    };
                }

                BoxAndReviewReset(term);
                term.State = TermState.Unknown;
                _termService.Save(term);

                Term newTerm = _termService.Find(term.Id);
                newTerm.AddIndividualTerm(new IndividualTerm() { Id = Guid.Empty, Created = DateTime.Now.AddYears(1) }, true);

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = "Reset, state is " + term.State.ToDescription().ToUpperInvariant(),
                        Data = new
                        {
                            termPhrase = term.TermPhrase.ToLowerInvariant(),
                            term = new JsonTermResult(newTerm)
                        }
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Save failed"
                    }
                };
            }
        }

        [AjaxRoute]
        public JsonResult MarkRemainingAsKnown(Guid languageId, Guid textId, IEnumerable<string> terms)
        {
            try
            {
                int count = 0;
                if(terms != null && terms.Any())
                {
                    terms = terms.Select(x => (x ?? "").Trim()).Where(x => x.Length > 0).Distinct(StringComparer.InvariantCultureIgnoreCase);

                    var language = _languageService.Find(languageId);
                    var currentTerms = _termService.FindAll(languageId).ToDictionary(x => x.TermPhrase.ToLowerInvariant(), x => x);
                    var termTest = new Regex(@"([" + language.Settings.RegexWordCharacters + @"])", RegexOptions.Compiled);

                    foreach(var t in terms)
                    {
                        var state = termTest.Match(t).Success ? TermState.Known : TermState.Ignored;
                        if(currentTerms.ContainsKey(t.ToLowerInvariant()))
                        {
                            if(currentTerms[t.ToLowerInvariant()].State == TermState.NotSeen)
                            {
                                //Terms can be explicitlity marked as not seen
                                var updateTerm = currentTerms[t.ToLowerInvariant()];
                                updateTerm.State = state;
                                _termService.Save(updateTerm);
                                count++;
                            }

                            continue;
                        }

                        //TODO fix me, bulk insert
                        Term term = NewTerm(languageId, t, state);
                        _termService.Save(term);
                        count++;
                    }
                }

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = string.Format("{0} words marked as known", count)
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Save failed"
                    }
                };
            }
        }

        [AjaxRoute]
        public JsonResult ReviewUnknown(Guid languageId, Guid textId, IEnumerable<string> terms)
        {
            try
            {
                int supplied = 0;
                int bumped = 0;
                List<string> message = new List<string>();

                if(terms != null && terms.Any())
                {
                    var review = _languageService.Find(languageId).Review ?? Review.Default;
                    terms = terms.Select(x => (x ?? "").Trim()).Where(x => x.Length > 0).Distinct(StringComparer.InvariantCultureIgnoreCase);

                    foreach(var t in terms)
                    {
                        var term = _termService.Find(languageId, t);

                        if(term == null)
                        {
                            continue;
                        }

                        supplied++;
                        var result = _termService.ReviewTerm(term, review);

                        if(result.Item1)
                        {
                            bumped++;
                            message.Add(result.Item2);
                        }
                    }
                }

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = string.Format("{0} terms supplied, {1} moved up a box<br/>{2}", supplied, bumped, string.Join("<br/>", message))
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Save failed"
                    }
                };
            }
        }

        [AjaxRoute]
        public JsonResult DeleteIndividualTerm(Guid termId, Guid individualTermId)
        {
            try
            {
                var term = _termService.Find(termId, true);

                if(term == null)
                {
                    throw new Exception("Term not found");
                }

                var it = term.IndividualTerms.FirstOrDefault(x => x.Id == individualTermId);

                if(it == null)
                {
                    throw new Exception("Individual term not found");
                }

                term.RemoveIndividualTerm(it.Id);
                _termService.Save(term);
                term.AddIndividualTerm(new IndividualTerm() { Id = Guid.Empty, Created = DateTime.Now.AddYears(1) }, true);

                return new JsonNetResult()
                {
                    Data = new ResponseMessage(OK)
                    {
                        Message = "Individual term deleted",
                        Data = new
                        {
                            termPhrase = term.TermPhrase.ToLowerInvariant(),
                            term = new JsonTermResult(term)
                        }
                    }
                };
            }
            catch(Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new JsonNetResult()
                {
                    Data = new ResponseMessage(FAIL)
                    {
                        Message = "Delete failed"
                    }
                };
            }
        }
    }
}