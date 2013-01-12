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
    public class SystemLanguage : IEntity
    {
        public const string NOT_SET_CODE = @"___";

        [BsonId]
        public ObjectId Id { get; set; }

        [StringLength(3)]
        public string Code { get; set; }

        [Required]
        [StringLength(60)]
        public string Name { get; set; }
    }
}
