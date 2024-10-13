using System.ComponentModel.DataAnnotations;

namespace Web_Chat.Models.Form
{
    public class EditProfileForm
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string? Bio { get; set; }
    }

}
