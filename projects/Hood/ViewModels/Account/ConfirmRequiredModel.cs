using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class ConfirmRequiredModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
