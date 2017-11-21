using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public partial class Address : BaseEntity, IAddress
    {

        public string ContactName { get; set; }
        public string QuickName { get; set; }

        [Display(Name = "Building Name/Number")]
        [Required]
        public string Number { get; set; }

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
        public ApplicationUser User { get; set; }

        public string FullAddress
        {
            get
            {
                var output = Address1 + ", ";
                if (!string.IsNullOrEmpty(Address2))
                    output += Address2 + ", ";
                output += City + ", ";
                output += County + ", ";
                output += Country + ", ";
                output += Postcode;
                return output;
            }
        }
        public bool IsDelivery { get { return User != null ? Id == User.DeliveryAddress?.Id : false; } }
        public bool IsBilling { get { return User != null ? Id == User.BillingAddress?.Id : false; } }

    }
}