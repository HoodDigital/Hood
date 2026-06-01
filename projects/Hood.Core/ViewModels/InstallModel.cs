using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    /// <summary>
    /// Backs the first-run install wizard (<c>/install</c>). Collects the administrator
    /// credentials used to seed the site owner. The email defaults to the configured
    /// <c>Hood:SuperAdminEmail</c> but can be overridden during setup.
    /// </summary>
    public class InstallModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Administrator email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
