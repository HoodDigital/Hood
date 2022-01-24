using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class LoginViewModel : SpamPreventionModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class MagicLoginViewModel : SpamPreventionModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
