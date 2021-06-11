using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class LoginViewModel : HoneyPotFormModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class MagicLoginViewModel : HoneyPotFormModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
