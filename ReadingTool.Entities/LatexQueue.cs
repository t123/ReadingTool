using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadingTool.Entities
{
    public class LatexQueue
    {
        public const string CollectionName = @"LatexQueue";

        [BsonId]
        public ObjectId LatexQueueId { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId ItemId { get; set; }
        public string Latex { get; set; }
        public DateTime Created { get; set; }
        public ObjectId FileId { get; set; }
    }
}
