using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Services;
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

        public ReadingController(ITextService textService, ITermService termService, ILanguageService languageService, IPrincipal principal)
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
            catch
            {
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

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
            catch
            {
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

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
            catch
            {
                return new JsonNetResult() { Data = new ResponseMessage(FAIL) };
            }
        }

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
            catch
            {
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

                IndividualTerms = term.IndividualTerms.Select(t => new JsonIndividualTermResult(t)).ToArray();
            }
        }

        public JsonResult FindTerm(Guid languageId, string termPhrase)
        {
            var term = _termService.Find(languageId, termPhrase);

            if(term == null)
            {
                term = new Term()
                    {
                        State = TermState.Unknown,
                    };
            }

            term.AddIndividualTerm(new IndividualTerm() { Id = Guid.Empty }, true);

            //term = new Term()
            //    {
            //        Id = new Guid("00000000-0000-0000-0000-000000000001"),
            //        LanguageId = languageId,
            //        Owner = _identity.UserId,
            //        Length = 1,
            //        TermPhrase = "lorem",
            //        State = TermState.Known,
            //        Box = 2,
            //        NextReview = DateTime.Now.AddDays(3)
            //    };

            //for(int i = 1; i <= 3; i++)
            //{
            //    term.AddIndividualTerm(new IndividualTerm()
            //        {
            //            BaseTerm = "lorem base" + i,
            //            Created = DateTime.Now.AddDays(-3),
            //            Modified = DateTime.Now.AddDays(-3).AddHours(i * 18),
            //            Id = new Guid(i + "0000000-0000-0000-0000-000000000000"),
            //            Definition = "Definition " + i,
            //            Romanisation = "Romanisation" + i,
            //            Sentence = "Sentence " + i,
            //            Tags = "tag" + i + "-1 tag" + i + "-2 tag" + i + "-3",
            //            TextId = new Guid("7dc4a5f7-43ef-6509-c16b-39bef0a24203"),
            //            TermId = new Guid("00000000-0000-0000-0000-000000000001"),
            //        }
            //        );
            //}

            //term.AddIndividualTerm(new IndividualTerm()
            //    {
            //        BaseTerm = "",
            //        Sentence = "",
            //        Romanisation = "",
            //        Tags = "",
            //        TextId = new Guid("7dc4a5f7-43ef-6509-c16b-39bef0a24203"),
            //        TermId = new Guid("00000000-0000-0000-0000-000000000001"),
            //        Id = Guid.Empty,
            //    });

            return new JsonNetResult() { Data = new JsonTermResult(term) };
        }

        public JsonResult Quicksave(Guid languageId, Guid textId, string termPhrase, string sentence, string state)
        {
            throw new NotImplementedException();
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

        public JsonResult SaveTerm(Guid languageId, Guid textId, Guid termId, string state, string termPhrase, JsonSaveTerm[] model)
        {
            Term term = _termService.Find(languageId, termPhrase);

            if(term == null)
            {
                term = new Term();
                term.Box = 1;
                term.Id = Guid.Empty;
                term.LanguageId = languageId;
                term.Length = 1;
                term.NextReview = null;
                term.State = Constants.ClassToTermStates[state];
                term.TermPhrase = termPhrase;
            }
            else
            {
                term.State = Constants.ClassToTermStates[state];
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
                            TextId = textId
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
                        Tags = it.Tags
                    });
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
                                    Box = term.Box.HasValue ? term.Box.Value.ToString() : "NA"
                                }
                        }
                };
        }
    }
}