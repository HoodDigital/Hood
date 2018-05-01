using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models.Payments
{
    public partial class BaseOrder<TAddress> : BaseEntity<string>
        where TAddress : IAddress, new()
    {

        [NotMapped]
        public TAddress BillingAddress
        {
            get { return _BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(_BillingAddressJson) : default(TAddress); }
            set { _BillingAddressJson = JsonConvert.SerializeObject(value); }
        }
        [NonSerialized]
        private string _BillingAddressJson;

        [JsonIgnore]
        public bool UseBilling { get; set; }

        [NotMapped]
        public TAddress ShippingAddress
        {
            get { return !UseBilling && _ShippingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(_ShippingAddressJson) : default(TAddress); }
            set { _ShippingAddressJson = JsonConvert.SerializeObject(value); }
        }
        [NonSerialized]
        private string _ShippingAddressJson;
    }
}
