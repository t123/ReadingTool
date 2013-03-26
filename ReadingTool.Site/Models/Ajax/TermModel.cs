using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Ajax
{
    public class TermModel
    {
        public long TermId { get; set; }
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
            get { return TermId != 0; }
        }

        public short Box { get; set; }
    }

    public class SaveTermModel
    {
        public long? TermId { get; set; }
        public long LanguageId { get; set; }
        public long TextId { get; set; }
        public string State { get; set; }
        [MaxLength(50)]
        public string Phrase { get; set; }
        [MaxLength(50)]
        public string BasePhrase { get; set; }

        [MaxLength(500)]
        public string Sentence { get; set; }

        [MaxLength(500)]
        public string Definition { get; set; }
        public string Tags { get; set; }
    }
}