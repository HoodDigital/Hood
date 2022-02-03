using Hood.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class Person : IName, IAddress, IPerson
    {
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Display(Name = "Display name")]
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public bool Anonymous { get; set; }
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Contact Name")]
        public string ContactName { get => this.ToInternalName(); set { } }
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
    }

}
