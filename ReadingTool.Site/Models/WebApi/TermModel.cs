using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ReadingTool.Core.Enums;
using ReadingTool.Entities;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace ReadingTool.Site.Models.WebApi
{
    public class TermModel
    {
        public Guid Id { get; set; }
        public Guid LanguageId { get; set; }
        public string LanguageName { get; set; }
        public string TermPhrase { get; set; }
        public TermState State { get; set; }
        public int? Box { get; set; }
        public DateTime? NextReview { get; set; }
        public IEnumerable<IndividualTermModel> IndividualTerms { get; set; }

        public TermModel()
        {
            IndividualTerms = new List<IndividualTermModel>();
        }
    }
}