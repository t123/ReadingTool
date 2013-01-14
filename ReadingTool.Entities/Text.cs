using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Core.Database;

namespace ReadingTool.Entities
{
    public class Text : IEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonIgnore]
        public DateTime Created { get { return Id.CreationTime; } }

        [Required]
        public DateTime Modified { get; set; }

        [Required]
        public ObjectId Owner { get; set; }

        [Required]
        public ObjectId L1Id { get; set; }

        public ObjectId? L2Id { get; set; }

        private string _title;

        [Required]
        [StringLength(100)]
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                TitleLower = _title == null ? null : _title.ToLowerInvariant();
            }
        }

        public string TitleLower { get; private set; }

        private string _collectionName;

        [StringLength(100)]
        public string CollectionName
        {
            get { return _collectionName; }
            set
            {
                _collectionName = value;
                CollectionNameLower = _collectionName == null ? null : _collectionName.ToLowerInvariant();
            }
        }

        public string CollectionNameLower { get; private set; }

        public int? CollectionNo { get; set; }

        [StringLength(250)]
        public string AudioUrl { get; set; }

        [BsonIgnore]
        public string L1Text { get; set; }

        private string _l2Text;
        [BsonIgnore]
        public string L2Text
        {
            get { return _l2Text; }
            set
            {
                _l2Text = value;
                IsParallel = !string.IsNullOrWhiteSpace(value);
            }
        }

        [Required]
        public bool IsParallel { get; set; }

        public string[] Tags { get; set; }

        public TextMetadata Metadata { get; set; }

        public Text()
        {
            Metadata = new TextMetadata();
            Tags = new string[0];
        }
    }
}
