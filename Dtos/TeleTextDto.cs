using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotTrungThuong.Dtos
{
    public class TeleTextDto
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string JoinedReply { get; set; }

        public string JoinSuccessReply { get; set; }

        public bool IsDeleted { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
