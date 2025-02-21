using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotTrungThuong.Dtos
{
    public class ThamGiaTrungThuongDto
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId ThietLapId { get; set; }

        public string ThietLapName { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string Fullname { get; set; }

        public bool IsDeleted { get; set; } = false;
        public long FromChatId { get; set; }
        public int FromMessageId { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
