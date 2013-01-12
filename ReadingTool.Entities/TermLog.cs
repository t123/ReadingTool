using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using ReadingTool.Core.Enums;

namespace ReadingTool.Entities
{
    public class TermLog
    {
        public ObjectId TermId { get; set; }
        public ObjectId LanguageId { get; set; }
        public ObjectId Owner { get; set; }
        public TermState State { get; set; }
        public DateTime Date { get; set; }
        public bool IsNew { get; set; }
        public bool StateChange { get; set; }

        //public Language Language { get; set; }
        //public Term Term { get; set; }
    }
}