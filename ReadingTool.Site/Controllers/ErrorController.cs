using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }

        public ActionResult PageNotFound()
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(new HttpException(404, "Page not found: " + Request.RawUrl));
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View("_NotFound");
        }

#if DEBUG
        public ActionResult TestInternal()
        {
            throw new Exception("Internal 500 error");
        }

        public ActionResult Test404()
        {
            throw new HttpException(404, "Page not found");
        }
#endif
    }
}
