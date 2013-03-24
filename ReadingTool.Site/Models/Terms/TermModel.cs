using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ReadingTool.Common;

namespace ReadingTool.Site.Models.Terms
{
    public class TermModel
    {
        public Guid TermId { get; set; }
        public TermState State { get; set; }

        [MaxLength(50, ErrorMessage = "Please use less than 50 characters")]
        public string Phrase { get; set; }

        [Display(Name = "Base Phrase")]
        [MaxLength(50, ErrorMessage = "Please use less than 50 characters")]
        public string BasePhrase { get; set; }

        [MaxLength(500, ErrorMessage = "Please use less than 500 characters")]
        public string Sentence { get; set; }

        [MaxLength(500, ErrorMessage = "Please use less than 500 characters")]
        public string Definition { get; set; }
        public string Tags { get; set; }
        public short Length { get; set; }
        public short Box { get; set; }
    }
}