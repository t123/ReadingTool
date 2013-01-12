using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadingTool.Core.Database
{
    public interface IEntity
    {
        [BsonId]
        ObjectId Id { get; }
    }
}