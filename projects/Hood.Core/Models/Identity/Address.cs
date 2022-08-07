using Hood.Entities;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class Address : BaseEntity, IAddress
    {
        public string ContactName { get; set; }
        public string QuickName { get; set; }

        [Display(Name = "Building Name/Number")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Required]
        public string City { get; set; }
        [Required]
        public string County { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string UserId { get; set; }
    }
}