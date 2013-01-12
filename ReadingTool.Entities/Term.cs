using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Core.Enums;

namespace ReadingTool.Entities
{
    public class Term
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

        private readonly List<IndividualTerm> _individualTerms;

        //[BsonIgnore]
        public IEnumerable<IndividualTerm> IndividualTerms
        {
            get { return _individualTerms.Where(x => !x.IsDeleted).OrderBy(x => x.Created).ToList().AsReadOnly(); }
            //set { _individualTerms = (value ?? new List<IndividualTerm>()).ToList(); }
        }

        [BsonIgnore]
        public IEnumerable<IndividualTerm> DeletedIndividualTerms
        {
            get { return _individualTerms.Where(x => x.IsDeleted); }
        }

        [BsonIgnore]
        public string Definition
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if(_individualTerms.Count == 1)
                {
                    var it = _individualTerms[0];
                    sb.AppendFormat("{0}{1}", it.BaseTerm, "\n");
                    if(!string.IsNullOrEmpty(it.Romanisation)) sb.AppendFormat("{0}{1}", it.Romanisation, "\n");
                    sb.AppendFormat("{0}{1}", it.Definition, "\n");
                }
                else
                {
                    int counter = 1;
                    foreach(var it in _individualTerms)
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
            _individualTerms = new List<IndividualTerm>();
        }

        #region individual terms
        public void AddIndividualTerms(IEnumerable<IndividualTerm> individualTerms)
        {
            _individualTerms.AddRange(individualTerms);
        }

        public void AddIndividualTerm(IndividualTerm individualTerm, bool allowBlank = false)
        {
            if(individualTerm == null)
            {
                return;
            }

            if(
                !allowBlank &&
                string.IsNullOrWhiteSpace(individualTerm.Definition) &&
                string.IsNullOrWhiteSpace(individualTerm.Romanisation) &&
                string.IsNullOrWhiteSpace(individualTerm.BaseTerm)
                )
            {
                //skip word where nothing is filled in
                return;
            }

            individualTerm.LanguageId = this.LanguageId;
            _individualTerms.Add(individualTerm);
        }

        public void RemoveIndividualTerm(ObjectId id)
        {
            RemoveIndividualTerm(_individualTerms.FirstOrDefault(x => x.Id == id));
        }

        public void RemoveIndividualTerm(IndividualTerm individualTerm)
        {
            if(individualTerm == null)
            {
                return;
            }

            individualTerm.IsDeleted = true;
        }

        public void UpdateIndividualTerm(ObjectId id, IndividualTerm individualTerm)
        {
            var it = _individualTerms.FirstOrDefault(x => x.Id == id);

            if(it == null)
            {
                return;
            }

            it.BaseTerm = individualTerm.BaseTerm;
            it.Definition = individualTerm.Definition;
            it.Sentence = individualTerm.Sentence;
            it.Tags = individualTerm.Tags;
            it.Romanisation = individualTerm.Romanisation;
            it.LanguageId = individualTerm.LanguageId;
        }
        #endregion
    }

    public class IndividualTerm
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime Created { get { return Id.CreationTime; } }
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
        [StringLength(1000)]
        public string Tags { get; set; }

        public IndividualTerm()
        {
            Id = ObjectId.GenerateNewId();
            BaseTerm = string.Empty;
            Sentence = string.Empty;
            Definition = string.Empty;
            Romanisation = string.Empty;
            Tags = string.Empty;
        }
    }
}
