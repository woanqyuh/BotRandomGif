using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Models
{
    public class DanhSachTrungThuongViewModel
    {
        public string Id { get; set; }

        public string ThietLapId { get; set; }

        public string ThietLapName { get; set; }
        public string UserId { get; set; }

        public string Username { get; set; }
        public string ScheduleName { get; set; }
        
        public string Fullname { get; set; }

        public string Gift { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class DanhSachTrungThuongOnlineViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        public string Username { get; set; }
        public string Fullname { get; set; }

        public string Gift { get; set; }

        public string GiftId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class CreateDanhSachTrungThuongOnlineReuqest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }


        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }


        [Required(ErrorMessage = "Fullname is required.")]
        public string Fullname { get; set; }


        [Required(ErrorMessage = "FromChatId is required.")]
        public long FromChatId { get; set; }


        [Required(ErrorMessage = "FromMessageId is required.")]
        public int FromMessageId { get; set; }


        [Required(ErrorMessage = "ThietLapId is required.")]
        public string ThietLapId { get; set; }


        [Required(ErrorMessage = "GiftId is required.")]
        public string GiftId { get; set; }

    }

    public class SendMessageRequest
    {
        [Required(ErrorMessage = "Ids is required.")]
        public string[] Ids { get; set; }
    }

    public class TrungThuongFilter
    {
        public string? GiftId { get; set; }
    }
}