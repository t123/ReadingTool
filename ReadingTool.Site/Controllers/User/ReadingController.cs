using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core;
using ReadingTool.Core.Formatters;
using ReadingTool.Services;
using ServiceStack;

namespace ReadingTool.Site.Controllers.User
{
    [ValidateInput(false)]
    public class ReadingController : Controller
    {
        private readonly ITextService _textService;
        private readonly ITermService _termService;
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

        public ReadingController(ITextService textService, ITermService termService)
        {
            _textService = textService;
            _termService = termService;
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

                return new JsonResult() { Data = new ResponseMessage(OK) };
            }
            catch
            {
                return new JsonResult() { Data = new ResponseMessage(FAIL) };
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

                return new JsonResult()
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
                return new JsonResult() { Data = new ResponseMessage(FAIL) };
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

                return new JsonResult()
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
                return new JsonResult() { Data = new ResponseMessage(FAIL) };
            }
        }

        public JsonResult FindTerm(Guid languageId, string termPhrase)
        {
            var term = _termService.Find(languageId, termPhrase);

            return new JsonResult() { Data = term };
        }

        public JsonResult Quicksave(Guid languageId, Guid textId, string term, string sentence, string state)
        {
            throw new NotImplementedException();
        }

        public JsonResult SaveTerm()
        {
            return new JsonResult()
                {
                    Data = new ResponseMessage(OK)
                        {
                            Message = "Saved, state is UNKNOWN",
                            Data = new
                                {
                                    Box = 1
                                }
                        }
                };
        }
    }
}