using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Models
{
    public class GiaiThuongViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public bool IsDeleted { get; set; }
        public string ImageUrl { get; set; }
        public int? WinnersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Time { get; set; }
        public bool IsShow { get; set; }
        public List<ChildrenGiftViewModel> ChildrenGifts { get; set; }
    }

    public class ChildrenGiftViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int WinnersCount { get; set; }
    }

    public class GiaiThuongRequest
    {
        public string Name { get; set; }

        [Required]
        public bool IsShow { get; set; }
        public string ImageUrl { get; set; }
        public List<ChildrenGiftRequest>? ChildrenGifts { get; set; }
        public int? Quantity { get; set; }

        public int Time { get; set; }

    }

    public class ChildrenGiftRequest
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

}
