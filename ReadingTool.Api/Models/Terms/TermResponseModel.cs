using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReadingTool.Common;

namespace ReadingTool.Api.Models.Terms
{
    public class TermResponseModel
    {
        public long TermId { get; set; }
        public string State { get; set; }
        public string Phrase { get; set; }
        public string PhraseLower { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public short Box { get; set; }
        public DateTime? NextReview { get; set; }
        public long TextId { get; set; }
        public long LanguageId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ICollection<string> Tags { get; set; }
        public short Length { get; set; }
    }
}