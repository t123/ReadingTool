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
        public string State { get; set; }
        public IList<IndividualTerm> IndividualTerms { get; set; }
        public string Tags
        {
            get { return string.Join(" ", IndividualTerms.Select(x => x.Tags).Distinct(StringComparer.InvariantCultureIgnoreCase)); }
        }

        public TermListModel()
        {
            IndividualTerms = new List<IndividualTerm>();
        }

        public class IndividualTerm
        {
            public Guid Id { get; set; }
            public string BaseTerm { get; set; }
            public string Sentence { get; set; }
            public string Definition { get; set; }
            public string Romanisation { get; set; }
            public string Tags { get; set; }
        }
    }
}