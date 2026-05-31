using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace APIServer.Models
{
    public class Scoreboard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; }

        public int Version { get; set;}

    }
}