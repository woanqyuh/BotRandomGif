using System.ComponentModel.DataAnnotations;

namespace BotTrungThuong.Models
{
    public class TeleTextModel
    {
        public string Id { get; set; }

        public string JoinedReply { get; set; }

        public string JoinSuccessReply { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TeleTextRequest
    {
        [Required(ErrorMessage = "JoinedReply is required.")]
        public string JoinedReply { get; set; }

        [Required(ErrorMessage = "JoinSuccessReply is required.")]
        public string JoinSuccessReply { get; set; }
    }
}
