using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Entities;
using ReadingTool.Entities.Search;
using ReadingTool.Services;
using ReadingTool.Site.Models.WebApi;

namespace ReadingTool.Site.Controllers.Api
{
    [Authorize(Roles = Constants.Roles.WEB)]
    public class TextsController : ApiController
    {
        private readonly ITextService _textService;

        public TextsController(ITextService textService)
        {
            _textService = textService;
        }

        public HttpResponseMessage GetTexts(int page = 1, int limit = 20)
        {
            var result = _textService.FilterTexts(
                new SearchOptions()
                {
                    Filter = "",
                    Page = page,
                    RowsPerPage = limit,
                    Direction = GridSortDirection.Asc,
                    Sort = ""
                }
                );

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Total = result.TotalRows,
                    Results = Mapper.Map<IEnumerable<Text>, IEnumerable<TextModel>>(result.Results)
                });
        }

        [ActionName("FindByFilter")]
        [HttpGet]
        public HttpResponseMessage FindTextsByFilter(string id, int page = 1, int limit = 20)
        {
            var result = _textService.FilterTexts(
                new SearchOptions()
                    {
                        Filter = id,
                        Page = page,
                        RowsPerPage = limit,
                        Direction = GridSortDirection.Asc,
                        Sort = ""
                    }
                );

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                    {
                        Total = result.TotalRows,
                        Results = Mapper.Map<IEnumerable<Text>, IEnumerable<TextModel>>(result.Results)
                    });
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
