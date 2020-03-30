using Newtonsoft.Json;
using System;

namespace Hood.Models.Payments
{
    public class SagePayMerchantSessionKey
    {
        [JsonProperty(PropertyName = "merchantSessionKey")]
        public string Key { get; set; }
        public DateTime Expiry { get; set; }
    }
}