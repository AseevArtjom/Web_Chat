using System.ComponentModel.DataAnnotations;

namespace Web_Chat.Models.Form
{
    public class RegisterForm : LoginForm
    {
        [Required]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }
    }
}
