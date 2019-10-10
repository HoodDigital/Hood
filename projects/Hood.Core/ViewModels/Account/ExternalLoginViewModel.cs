using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
