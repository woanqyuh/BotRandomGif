using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Dtos
{
    [BsonIgnoreExtraElements]
    public class ThietLapTrungThuongDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public string Content { get; set; }

        public string ChatId { get; set; }
        public string[] Command { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ObjectId CreatedBy { get; set; }

        // Danh sách các khung giờ trả thưởng và phần thưởng tương ứng
        public List<RewardSchedule> RewardSchedules { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class RewardSchedule
    {
        public string Name { get; set; }
        public DateTime ResultTime { get; set; }
        public List<RewardItem> Rewards { get; set; } = new();
    }

    [BsonIgnoreExtraElements]
    public class RewardItem
    {
        public string Gift { get; set; }
        public int Quantity { get; set; }
    }
}
