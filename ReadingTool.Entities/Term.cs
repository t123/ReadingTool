using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Core.Database;
using ReadingTool.Core.Enums;

namespace ReadingTool.Entities
{
    public class Term : IEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        public ObjectId LanguageId { get; set; }

        [Required]
        public ObjectId Owner { get; set; }

        [Required]
        [StringLength(50)]
        public string TermPhrase { get; set; }

        [Required]
        public short Length { get; set; }

        [BsonIgnore]
        public bool StateHasChanged { get; set; }
        private TermState _state { get; set; }

        [Required]
        [StringLength(8)]
        public TermState State
        {
            get { return _state; }
            set
            {
                if(Id != ObjectId.Empty && _state != value && _state != 0)
                {
                    StateHasChanged = true;
                }

                _state = value;
            }
        }

        public int? Box { get; set; }

        public DateTime? NextReview { get; set; }

        public IList<IndividualTerm> IndividualTerms { get; set; }

        [BsonIgnore]
        public string Definition
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if(IndividualTerms.Count() == 1)
                {
                    var it = IndividualTerms.First();
                    sb.AppendFormat("{0}{1}", it.BaseTerm, "\n");
                    if(!string.IsNullOrEmpty(it.Romanisation)) sb.AppendFormat("{0}{1}", it.Romanisation, "\n");
                    sb.AppendFormat("{0}{1}", it.Definition, "\n");
                }
                else
                {
                    int counter = 1;
                    foreach(var it in IndividualTerms)
                    {
                        if(it.Id == ObjectId.Empty)
                        {
                            continue;
                        }

                        sb.AppendFormat("Definition {0}{1}", counter++, "\n");
                        sb.AppendFormat("{0}{1}", it.BaseTerm, "\n");
                        if(!string.IsNullOrEmpty(it.Romanisation)) sb.AppendFormat("{0}{1}", it.Romanisation, "\n");
                        sb.AppendFormat("{0}{1}", it.Definition, "\n");
                        sb.AppendLine();
                    }
                }

                return sb.ToString().Trim();
            }
        }

        public Term()
        {
            IndividualTerms = new List<IndividualTerm>();
        }
    }

    public class IndividualTerm
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        [BsonIgnore]
        public bool IsDeleted { get; set; }

        [Required]
        public ObjectId? TextId { get; set; }

        [Required]
        public ObjectId LanguageId { get; set; }

        [Required]
        [StringLength(50)]
        public string BaseTerm { get; set; }

        [Required]
        [StringLength(500)]
        public string Sentence { get; set; }

        [Required]
        [StringLength(1000)]
        public string Definition { get; set; }

        [Required]
        [StringLength(50)]
        public string Romanisation { get; set; }

        [Required]
        public string[] Tags { get; set; }

        public IndividualTerm()
        {
            Id = ObjectId.GenerateNewId();
            BaseTerm = string.Empty;
            Sentence = string.Empty;
            Definition = string.Empty;
            Romanisation = string.Empty;
            Tags = new string[] { };
        }
    }
}
