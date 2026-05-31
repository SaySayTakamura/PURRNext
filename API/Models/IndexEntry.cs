using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIServer.Models
{
    public class IndexEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; }
    }
}