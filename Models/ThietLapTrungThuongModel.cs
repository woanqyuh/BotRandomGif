using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Models
{
    public class ThietLapTrungThuongViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string[] Command { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string Content { get; set; }
        public string ChatId { get; set; }
        public int Status { get; set; }

        public List<RewardScheduleViewModel> RewardSchedules { get; set; }
    }

    public class RewardScheduleViewModel
    {
        public string Name { get; set; }
        public DateTime ResultTime { get; set; } 
        public List<RewardViewModel> Rewards { get; set; } 
    }

    public class RewardViewModel
    {
        public string Gift { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateThietLapTrungThuongRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Command is required.")]
        public string[] Command { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "ChatId is required.")]
        public string ChatId { get; set; }

        [Required(ErrorMessage = "At least one reward schedule is required.")]
        public List<RewardScheduleRequest> RewardSchedules { get; set; }
    }

    public class RewardScheduleRequest
    {

        [Required(ErrorMessage = "ResultTime is required.")]
        public DateTime ResultTime { get; set; }

        [Required(ErrorMessage = "At least one reward is required.")]
        public List<RewardRequest> Rewards { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
    }

    public class RewardRequest
    {
        [Required(ErrorMessage = "Gift name is required.")]
        public string Gift { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
    }
}
