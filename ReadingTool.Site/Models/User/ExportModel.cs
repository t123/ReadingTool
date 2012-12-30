using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Entities;

namespace ReadingTool.Site.Models.User
{
    public class ExportModel
    {
        public Entities.User User { get; set; }
        public IList<Language> Languages { get; set; }
        public IList<Term> Terms { get; set; }
        public IList<Text> Texts { get; set; }
        public IList<SystemLanguage> SystemLanguages{ get; set; }

        public ExportModel()
        {
            SystemLanguages = new List<SystemLanguage>();
        }
    }
}