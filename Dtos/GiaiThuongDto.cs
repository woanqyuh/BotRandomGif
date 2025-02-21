using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Dtos
{
    [BsonIgnoreExtraElements]
    public class GiaiThuongDto
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public string ImageUrl { get; set; }
        public int? WinnersCount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool? IsShow { get; set; }
        public int? Time { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChildrenGift>? ChildrenGifts { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ChildrenGift
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public int WinnersCount { get; set; } = 0;

    }

}
