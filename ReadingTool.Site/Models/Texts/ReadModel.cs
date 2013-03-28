using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Languages;

namespace ReadingTool.Site.Models.Texts
{
    public class ReadModel
    {
        public bool AsParallel { get; set; }
        public string ParsedText { get; set; }
        public Tuple<long?, long?> PagedTexts { get; set; }

        public LanguageViewModel Language { get; set; }
        public LanguageViewModel Language2 { get; set; }
        public AccountModel.UserModel User { get; set; }
        public TextViewModel Text { get; set; }
    }
}