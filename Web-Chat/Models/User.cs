using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Web_Chat.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [MaxLength(80)]
        public string? Location { get; set; }

        public DateOnly? Birthday { get; set; }

        [MaxLength(250)]
        public string? Bio { get; set; }

        public int? ImageId { get; set; }
        public virtual ImageFile? Image { get; set; }

        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public virtual ICollection<Message> SentMessages { get; set; }
    }
}
