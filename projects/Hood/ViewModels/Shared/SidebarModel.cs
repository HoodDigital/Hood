using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class SidebarModel : HeaderModel
    {

        // Login features
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

    }
}
