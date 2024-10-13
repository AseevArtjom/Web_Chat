using System.Text.Json.Serialization;

namespace Web_Chat.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public int ChatId { get; set; }

        [JsonIgnore]
        public virtual Chat Chat { get; set; }
        public int SenderId { get; set; }

        [JsonIgnore]
        public virtual User Sender { get; set; }
    }
}
