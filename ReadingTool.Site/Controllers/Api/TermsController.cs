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
    public class TermsController : ApiController
    {
        private readonly ITermService _termService;

        public TermsController(ITermService termService)
        {
            _termService = termService;
        }

        public HttpResponseMessage GetTerms(int page = 1, int limit = 20)
        {
            var result = _termService.FilterTerms(
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
                    Results = Mapper.Map<IEnumerable<Term>, IEnumerable<TermModel>>(result.Results)
                });
        }

        [ActionName("FindByFilter")]
        [HttpGet]
        public HttpResponseMessage FindTermsByFilter(string id, int page = 1, int limit = 20)
        {
            var result = _termService.FilterTerms(
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
                    Results = Mapper.Map<IEnumerable<Term>, IEnumerable<TermModel>>(result.Results)
                });
        }

        public TermModel GetTermById(Guid id)
        {
            var term = _termService.Find(id);
            if(term == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<Term, TermModel>(term);
        }
    }
}
