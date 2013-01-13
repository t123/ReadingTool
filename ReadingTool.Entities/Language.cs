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
    public class Language : IEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [Required]
        public DateTime Created { get { return Id.CreationTime; } }

        [Required]
        public DateTime Modified { get; set; }

        [Required]
        public ObjectId Owner { get; set; }

        [Required]
        [StringLength(7, MinimumLength = 7)]
        public string Colour { get; set; }

        [Required]
        public LanguageSettings Settings { get; set; }

        public ObjectId? SystemLanguageId { get; set; }

        public IList<LanguageDictionary> Dictionaries { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [StringLength(250)]
        public Review Review { get; set; }

        public Language()
        {
            Dictionaries = new List<LanguageDictionary>();
        }
    }
}
