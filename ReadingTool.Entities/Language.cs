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

        private List<LanguageDictionary> _dictionaries;
        public IEnumerable<LanguageDictionary> Dictionaries
        {
            get { return _dictionaries.AsReadOnly(); }
            set { _dictionaries = (value ?? new List<LanguageDictionary>()).ToList(); }
        }

        [Required]
        public bool IsPublic { get; set; }

        [StringLength(250)]
        public Review Review { get; set; }

        public Language()
        {
            _dictionaries = new List<LanguageDictionary>();
        }

        #region dictionaries
        public void AddDictionary(LanguageDictionary dictionary)
        {
            if(dictionary == null)
            {
                return;
            }

            _dictionaries.Add(dictionary);
        }

        public void UpdateDictionary(LanguageDictionary dictionary)
        {
            if(dictionary == null)
            {
                return;
            }

            _dictionaries.Remove(_dictionaries.FirstOrDefault(x => x.Id == dictionary.Id));
            _dictionaries.Add(dictionary);
        }

        public void RemoveDictionary(ObjectId id)
        {
            RemoveDictionary(_dictionaries.FirstOrDefault(x => x.Id == id));
        }

        public void RemoveDictionary(LanguageDictionary dictionary)
        {
            _dictionaries.Remove(dictionary);
        }
        #endregion
    }
}
