using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Entities;

namespace ReadingTool.Site.Models.User
{
    public class ReadViewModel
    {
        public bool AsParallel { get; set; }
        public string ParsedText { get; set; }
        public Tuple<Guid?, Guid?> PagedTexts { get; set; }

        public Language Language { get; set; }
        public Entities.User User { get; set; }
        public Text Text { get; set; }
    }
}