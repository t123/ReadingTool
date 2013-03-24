using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Terms
{
    public class TermViewModel
    {
        public Guid TermId { get; set; }
        public string State { get; set; }
        public string Phrase { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Created { get; set; }
        public string Modified { get; set; }
        public string Language { get; set; }
        public string FullDefinition { get; set; }
        public short Box { get; set; }
        public string NextReviewDate { get; set; }
    }
}