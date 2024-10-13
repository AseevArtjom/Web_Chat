using System.ComponentModel.DataAnnotations;

namespace Web_Chat.Models.Form
{
    public class LoginForm
    {
        [EmailAddress]
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
