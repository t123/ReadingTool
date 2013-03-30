using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Api.Models.Read
{
    public class TermRequestModel
    {
        public Guid? TermId { get; set; }
        public string Phrase { get; set; }
        public Guid LanguageId { get; set; }
    }

    public class TermResponseModel
    {
        public Guid TermId { get; set; }
        public string State { get; set; }
        public string Phrase { get; set; }
        public string BasePhrase { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Tags { get; set; }
        public string Message { get; set; }
        public string StateClass { get; set; }
        public short Length { get; set; }

        public bool Exists
        {
            get { return TermId != Guid.Empty; }
        }

        public short Box { get; set; }
    }
}