namespace BotTrungThuong.Models
{
    public class BotConfigurationViewModel
    {
        public string Id { get; set; }

        public string KeyValue { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BotConfigurationRequest
    {
        public string KeyValue { get; set; }
    }
}
