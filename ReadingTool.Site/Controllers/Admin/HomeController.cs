using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;

namespace ReadingTool.Site.Controllers.Admin
{
    [CustomAuthorize(Roles = Constants.Roles.ADMIN)]
    public class HomeController : Controller
    {
        private readonly ISystemLanguageService _systemLanguageService;

        public HomeController(ISystemLanguageService systemLanguageService)
        {
            _systemLanguageService = systemLanguageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if(file != null && file.ContentLength > 0)
            {
                int count = 0;
                const short CodeColumnNo = 0;
                const short LanguageNameColumnNo = 6;

                try
                {
                    IList<SystemLanguage> languages = new List<SystemLanguage>();
                    string csv;
                    var currentLanguages = _systemLanguageService.FindAll().ToDictionary(x => x.Code);
                    using(TextReader tr = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        csv = tr.ReadToEnd();
                    }

                    int i = 0;
                    foreach(string line in csv.Split('\n'))
                    {
                        string[] split = line.Split('\t');

                        if(i++ == 0)
                        {
                            try
                            {
                                var ccode = split[CodeColumnNo];
                                var cname = split[LanguageNameColumnNo];

                                if(ccode != "Id" || cname != "Ref_Name")
                                    throw new Exception("Are you sure this is the right file?");
                            }
                            catch
                            {
                                throw new Exception("Are you sure this is the right file?");
                            }
                            continue;
                        }

                        string code = split[CodeColumnNo];
                        if(currentLanguages.ContainsKey(code)) continue;

                        if(code.Length != 3 || split[LanguageNameColumnNo].Length > 60)
                        {
                            throw new Exception(string.Format("{0}/{1}", code, split[LanguageNameColumnNo]));
                        }

                        languages.Add(new SystemLanguage()
                        {
                            Id = SequentialGuid.NewGuid(),
                            Code = code,
                            Name = split[LanguageNameColumnNo]
                        });

                        count++;
                    }

                    _systemLanguageService.Save(languages.OrderBy(x => x.Name).ToArray());

                    this.FlashSuccess("{0} languages imported", count);

                    return RedirectToAction("Import");
                }
                catch(Exception e)
                {
                    this.FlashError("Exception: " + e);
                    return View();
                }
            }

            this.FlashError("Please upload a file");
            return View();
        }
    }
}
