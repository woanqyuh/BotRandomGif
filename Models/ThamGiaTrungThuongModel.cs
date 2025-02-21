using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BotTrungThuong.Models
{
    public class ThamGiaTrungThuongViewModel
    {
        public string Id { get; set; }

        public string ThietLapId { get; set; }

        public string ThietLapName { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string Fullname { get; set; }

        public long FromChatId { get; set; }
        public int FromMessageId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
