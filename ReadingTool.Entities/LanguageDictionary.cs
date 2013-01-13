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
    public class LanguageDictionary
    {
        //[BsonId]
        public ObjectId Id { get; set; }

        [StringLength(250)]
        [Required]
        public string Url { get; set; }

        [StringLength(30)]
        [Required]
        public string Name { get; set; }

        [StringLength(30)]
        public string WindowName { get; set; }

        [StringLength(10)]
        public string UrlEncoding { get; set; }
        public DictionaryParameter Parameter { get; set; }
        public bool AutoOpen { get; set; }
        public long DisplayOrder { get; set; }
    }
}
