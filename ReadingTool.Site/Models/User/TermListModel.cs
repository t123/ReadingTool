﻿using System;
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
        public string Definition { get; set; }
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

    public class TermExportModel
    {
        public Guid Id { get; set; }
        public Guid LanguageId { get; set; }
        public Guid? IndividualTermId { get; set; }
        public string LanguageName { get; set; }
        public string TermPhrase { get; set; }
        public int? Box { get; set; }
        public DateTime? NextReview { get; set; }
        public string State { get; set; }
        public string BaseTerm { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Romanisation { get; set; }
        public string Tags { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}",
                Id, LanguageId, IndividualTermId, LanguageName, TermPhrase, Box, NextReview,
                State, BaseTerm, Sentence, Definition, Romanisation, Tags
                );
        }
    }
}