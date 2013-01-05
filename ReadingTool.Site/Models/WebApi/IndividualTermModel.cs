using System;
using System.Collections.Generic;
using ReadingTool.Core.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Site.Models.WebApi
{
    public class IndividualTermModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public Guid? TextId { get; set; }
        public string BaseTerm { get; set; }
        public string Sentence { get; set; }
        public string Definition { get; set; }
        public string Romanisation { get; set; }
        public string[] Tags { get; set; }
    }
}