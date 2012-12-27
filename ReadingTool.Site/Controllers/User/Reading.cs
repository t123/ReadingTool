using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Controllers.User
{
    [ValidateInput(false)]
    public class ReadingController : Controller
    {
        [HttpPost]
        public ActionResult Index()
        {
            throw new NotSupportedException();
        }

        public ActionResult FindTerm(Guid languageId, string term)
        {
            throw new NotImplementedException();
        }

        public ActionResult Quicksave(Guid languageId, Guid itemId, string term, string sentence, string state)
        {
            throw new NotImplementedException();
        }
    }
}