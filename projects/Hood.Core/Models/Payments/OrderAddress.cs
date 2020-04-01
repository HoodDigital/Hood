using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.Models.Payments
{
    public class OrderAddress : IAddress
    {
        [JsonIgnore]
        public string ContactName { get; set; }
        [JsonIgnore]
        public string Number { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        [JsonProperty(PropertyName = "state")]
        public string County { get; set; }
        public string Country { get; set; }
        [JsonProperty(PropertyName = "postalCode")]
        public string Postcode { get; set; }
        [JsonIgnore]
        public double Latitude { get; set; }
        [JsonIgnore]
        public double Longitude { get; set; }
    }
}
