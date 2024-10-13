namespace Web_Chat.Models
{
    public class Chat
    {
        public Chat()
        {
            Members = new List<User>();
            Messages = new List<Message>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ImageFile? Avatar { get; set; }
        public virtual ICollection<User> Members { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
    }
}
