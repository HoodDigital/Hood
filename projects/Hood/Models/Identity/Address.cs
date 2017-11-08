using Hood.Entities;
using Hood.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public partial class Address : AddressBase
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public HoodIdentityUser User { get; set; }
    }
    public partial class Address<TUser> : AddressBase where TUser : IHoodUser
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public TUser User { get; set; }
    }
    public abstract class AddressBase : BaseEntity, IAddress
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
    }
}