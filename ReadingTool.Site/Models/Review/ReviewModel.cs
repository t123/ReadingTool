﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Site.Models.Terms;

namespace ReadingTool.Site.Models.Review
{
    public class ReviewModel
    {
        public IEnumerable<TermViewModel> Terms { get; set; }
        public int ReviewTotal { get; set; }
        public Dictionary<long, string> Languages { get; set; }
        public long LanguageId { get; set; }
    }
}