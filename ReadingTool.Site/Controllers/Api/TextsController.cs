using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Models.WebApi;

namespace ReadingTool.Site.Controllers.Api
{
    public class TextsController : ApiController
    {
        private readonly ITextService _textService;

        public TextsController(ITextService textService)
        {
            _textService = textService;
        }

        public IEnumerable<TextModel> GetTexts()
        {
            return Mapper.Map<IEnumerable<Text>, IEnumerable<TextModel>>(_textService.FindAll(true));
        }

        public IEnumerable<TextModel> GetTextsByLanguage(Guid languageId)
        {
            return Mapper.Map<IEnumerable<Text>, IEnumerable<TextModel>>(_textService.FindAllByLanguage(languageId, true));
        }

        public TextModel GetTextById(Guid id)
        {
            var text = _textService.Find(id);
            if(text == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<Text, TextModel>(text);
        }
    }
}
