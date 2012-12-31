using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class TermListModel
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public string LanguageColour { get; set; }
        public string TermPhrase { get; set; }
        public int? Box { get; set; }
        public DateTime? NextReview { get; set; }
        public string Tags { get; set; }
        public string State { get; set; }
    }
}