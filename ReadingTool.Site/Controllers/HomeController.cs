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

        public ActionResult ReimportData()
        {
            var connection = ContextPerRequest.Current;
            //connection.DropAndCreateTable<SystemLanguage>();

            if(connection.TableExists("TermLog"))
            {
                connection.DeleteAll<TermLog>();
                connection.DropTable<TermLog>();
            }

            if(connection.TableExists("LanguageSettings"))
            {
                connection.DeleteAll<LanguageSettings>();
                connection.DropTable<LanguageSettings>();
            }

            if(connection.TableExists("Tag"))
            {
                connection.DeleteAll<Tag>();
                connection.DropTable<Tag>();
            }

            if(connection.TableExists("IndividualTerm"))
            {
                connection.DeleteAll<IndividualTerm>();
                connection.DropTable<IndividualTerm>();
            }

            if(connection.TableExists("Term"))
            {
                connection.DeleteAll<Term>();
                connection.DropTable<Term>();
            }

            if(connection.TableExists("Text"))
            {
                connection.DeleteAll<Text>();
                connection.DropTable<Text>();
            }

            if(connection.TableExists("Language"))
            {
                connection.DeleteAll<Language>();
                connection.DropTable<Language>();
            }

            if(connection.TableExists("User"))
            {
                connection.DeleteAll<Entities.User>();
                connection.DropTable<Entities.User>();
            }

            if(connection.TableExists("Sequence"))
            {
                connection.DeleteAll<Sequence>();
                connection.DropTable<Sequence>();
            }

            connection.CreateTable<Entities.User>(true);
            connection.CreateTable<Language>(true);
            connection.CreateTable<Text>(true);
            connection.CreateTable<Term>(true);
            connection.CreateTable<IndividualTerm>(true);
            connection.CreateTable<Tag>(true);
            connection.CreateTable<Sequence>(true);
            connection.CreateTable<TermLog>(true);

            using(StreamReader sr = new StreamReader(Path.Combine(Server.MapPath("~/App_Data"), "dummy.sql"), Encoding.UTF8))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if(string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    connection.ExecuteSql(line);
                }
            }

            this.FlashSuccess("Data imported");
            return RedirectToAction("Index");
        }
    }
}
