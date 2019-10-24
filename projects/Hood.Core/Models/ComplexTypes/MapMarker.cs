using Hood.Interfaces;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public partial class MapMarker : IAddress
    {
        // IAddress
        public string ContactName { get; set; }
        public string Number { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string AssociatedId { get; set; }
        public string MarkerUrl { get; set; }
        public string ImageUrl { get; set; }

        public MapMarker(IAddress address, string title, string description, string id, string url, string imageUrl)
        {
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            County = address.County;
            Postcode = address.Postcode;
            Country = address.Country;
            Latitude = address.Latitude;
            Longitude = address.Longitude;
            Title = title;
            AssociatedId = id;
            MarkerUrl = url;
            Description = description;
            ImageUrl = imageUrl;
        }
    }
}

