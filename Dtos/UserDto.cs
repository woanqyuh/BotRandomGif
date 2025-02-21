using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotTrungThuong.Dtos
{
    [BsonIgnoreExtraElements]
    public class UserDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public bool IsDeleted { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ObjectId CreatedBy { get; set; }
    }
}