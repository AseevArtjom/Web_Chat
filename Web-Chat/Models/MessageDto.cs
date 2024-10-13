using Web_Chat.Migrations;

namespace Web_Chat.Models
{
    public class MessageDto
    {

        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public User Sender { get; set; }
        public int SenderId { get; set; }
        public int ChatId {  get; set; }
        public string SenderImageUrl { get; set; }
    }
}
