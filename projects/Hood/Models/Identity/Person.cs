using Hood.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class Person : IName, IAddress
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

        public string ContactName { get => this.ToFullName(); set { } }
        public string Number { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
