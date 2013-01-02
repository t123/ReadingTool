using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Entities;
using ReadingTool.Site.Helpers;
using ServiceStack.OrmLite;

namespace ReadingTool.Site.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }
    }
}
