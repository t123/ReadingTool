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
    public class User : IEntity
    {
        public static readonly ObjectId DummyOwner = ObjectId.Empty;

        [BsonId]
        public ObjectId Id { get; set; }

        [Required]
        [StringLength(30)]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [EmailAddress]
        [StringLength(50)]
        public string EmailAddress { get; set; }
        public DateTime Created { get { return Id.CreationTime; } }
        public DateTime Modified { get; set; }

        [Required]
        public string[] Roles { get; set; }

        private string _displayName;

        [StringLength(30)]
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(_displayName) ? Username : _displayName; }
            set { _displayName = (value ?? "").Trim(); }
        }

        [StringLength(10)]
        public string Theme { get; set; }

        [StringLength(1000)]
        public KeyBindings KeyBindings { get; set; }

        public Api Api { get; set; }

        public User()
        {
            KeyBindings = new KeyBindings();
            Api = new Api();
        }
    }
}
