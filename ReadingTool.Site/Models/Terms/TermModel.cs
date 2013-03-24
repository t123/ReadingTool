using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Common;

namespace ReadingTool.Site.Models.Terms
{
    public class TermModel
    {
        public Guid TermId { get; set; }
        public TermState State { get; set; }
        public string Phrase { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Tags { get; set; }
        public short Length { get; set; }
        public short Box { get; set; }
    }
}