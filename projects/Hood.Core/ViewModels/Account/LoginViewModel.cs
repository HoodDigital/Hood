using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class LoginViewModel : SpamPreventionModel
    {
        [Required]
        [EmailAddress]
        public virtual string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }

        [Display(Name = "Remember me?")]
        public virtual bool RememberMe { get; set; }
    }

    public class MagicLoginViewModel : SpamPreventionModel
    {
        [Required]
        [EmailAddress]
        public virtual string Email { get; set; }
        public virtual string ReturnUrl { get; set; }
    }
}
