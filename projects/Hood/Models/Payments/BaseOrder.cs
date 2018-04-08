using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hood.Models.Payments
{
    public partial class BaseOrder<TAddress> : BaseEntity<string>
        where TAddress : IAddress, new()
    {

        [NotMapped]
        public TAddress BillingAddress
        {
            get { return _BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(_BillingAddressJson) : new TAddress(); }
            set { _BillingAddressJson = JsonConvert.SerializeObject(value); }
        }
        [NonSerialized]
        private string _BillingAddressJson;

        [JsonIgnore]
        public bool UseBilling { get; set; }

        [NotMapped]
        public TAddress ShippingAddress
        {
            get { return _ShippingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(_ShippingAddressJson) : new TAddress(); }
            set { _ShippingAddressJson = JsonConvert.SerializeObject(value); }
        }
        [NonSerialized]
        private string _ShippingAddressJson;
    }
}
