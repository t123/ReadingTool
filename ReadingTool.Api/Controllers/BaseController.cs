using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ReadingTool.Api.Attributes;

namespace ReadingTool.Api.Controllers
{
    [NeedsPersistence]
    [System.Web.Http.Authorize]
    public class BaseController : ApiController
    {
        protected Guid UserId
        {
            get { return Guid.Parse(HttpContext.Current.User.Identity.Name); }
        }
    }
}
