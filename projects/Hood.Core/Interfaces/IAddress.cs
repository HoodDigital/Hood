using System.ComponentModel.DataAnnotations;

namespace Hood.Interfaces
{
    public interface IAddress
    {
        [Display(Name = "Contact Name")]
        string ContactName { get; set; }

        [Display(Name = "Building Name/Number")]
        string Number { get; set; }

        [Display(Name = "Address 1")]
        string Address1 { get; set; }

        [Display(Name = "Address 2")]
        string Address2 { get; set; }
        string City { get; set; }
        string County { get; set; }
        string Country { get; set; }
        string Postcode { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}

