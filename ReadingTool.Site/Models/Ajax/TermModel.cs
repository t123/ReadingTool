using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.Ajax
{
    public class TermModel
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

    public class SaveTermModel
    {
        public Guid? TermId { get; set; }
        public Guid LanguageId { get; set; }
        public Guid TextId { get; set; }
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