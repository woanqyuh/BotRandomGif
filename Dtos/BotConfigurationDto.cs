using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotTrungThuong.Dtos
{
    public class BotConfigurationDto
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string KeyValue { get; set; }

        public bool IsDeleted { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
