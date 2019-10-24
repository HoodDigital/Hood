using Hood.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class RegisterViewModel : HoneyPotFormModel, IName, IAddress, IPerson
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Consent")]
        public bool Consent { get; set; }
        public bool Anonymous { get; set; }
        public string FullName { get; set; }
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Contact Name")]
        public string ContactName { get => this.ToFullName(); set { } }
        public string Number { get; set; }
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [Display(Name = "Notify the user", Description = "Email the new account access information to the user.")]
        public bool NotifyUser { get; set; }
    }
}
