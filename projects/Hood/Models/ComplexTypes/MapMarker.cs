using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public string MarkerContent { get; set; }
        public string AssociatedId { get; set; }
        public string MarkerUrl { get; set; }

        public MapMarker(IAddress address, string content, string id, string url)
        {
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            County = address.County;
            Postcode = address.Postcode;
            Country = address.Country;
            Latitude = address.Latitude;
            Longitude = address.Longitude;
            MarkerContent = content;
            AssociatedId = id;
            MarkerUrl = url;
        }
    }
}

