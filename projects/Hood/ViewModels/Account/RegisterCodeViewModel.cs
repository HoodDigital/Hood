using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class RegisterCodeViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
