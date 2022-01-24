using Hood.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class AdminCreateUserViewModel : RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Create Validated Account", Description = "Create the user, and mark the email address and phone number as valid. This will override the requirement for the user to validate their email when logging in. This is not recommended to maintain account security.")]
        public bool CreateValidated { get; set; } = false;

        [Display(Name = "Notify the user", Description = "Email the new account access information to the user.")]
        public bool NotifyUser { get; set; }
    }

    public class PasswordRegisterViewModel : RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class MagicRegisterViewModel : RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public override string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public override string LastName { get; set; }
    }

    public class RegisterViewModel : SpamPreventionModel, IName, IAddress, IPerson
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public virtual string Email { get; set; }

        [Display(Name = "Phone Number")]
        public virtual string PhoneNumber { get; set; }

        [Display(Name = "Username")]
        public virtual string Username { get; set; }

        [Required]
        [Display(Name = "Consent")]
        public virtual bool Consent { get; set; }

        public virtual bool Anonymous { get; set; }

        public virtual string FullName { get; set; }

        [Display(Name = "Display Name")]
        public virtual string DisplayName { get; set; }

        [Display(Name = "First Name")]
        public virtual string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public virtual string LastName { get; set; }

        [Display(Name = "Phone Number")]
        public virtual string Phone { get; set; }

        [Display(Name = "Job Title")]
        public virtual string JobTitle { get; set; }

        [Display(Name = "Contact Name")]
        public virtual string ContactName { get => this.ToInternalName(); set { } }

        public virtual string Number { get; set; }

        [Display(Name = "Address 1")]
        public virtual string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public virtual string Address2 { get; set; }

        public virtual string City { get; set; }

        public virtual string County { get; set; }

        public virtual string Country { get; set; }

        public virtual string Postcode { get; set; }

        public virtual double Latitude { get; set; }

        public virtual double Longitude { get; set; }
    }
}
