using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace ReadingTool.Site.Controllers
{
    public class BaseController : Controller
    {
        protected Guid UserId
        {
            get { return Guid.Parse(HttpContext.User.Identity.Name); }
        }
    }
}
