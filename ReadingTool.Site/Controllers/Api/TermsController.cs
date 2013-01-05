using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Models.WebApi;

namespace ReadingTool.Site.Controllers.Api
{
    public class TermsController : ApiController
    {
        private readonly ITermService _termService;

        public TermsController(ITermService termService)
        {
            _termService = termService;
        }

        public IEnumerable<TermModel> GetTerms()
        {
            return Mapper.Map<IEnumerable<Term>, IEnumerable<TermModel>>(_termService.FindAll());
        }

        public TermModel GetTermById(Guid id)
        {
            return Mapper.Map<Term, TermModel>(_termService.Find(id));
        }
    }
}
