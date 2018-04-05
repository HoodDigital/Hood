using Hood.Entities;
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
            get
            {
                return new TAddress()
                {
                    Number = _BillingNumber,
                    Address1 = _BillingAddress1,
                    Address2 = _BillingAddress2,
                    City = _BillingCity,
                    County = _BillingCounty,
                    Country = _BillingCountry,
                    Postcode = _BillingPostcode,
                };
            }
            set
            {
                _BillingNumber = value.Number;
                _BillingAddress1 = value.Address1;
                _BillingAddress2 = value.Address2;
                _BillingCity = value.City;
                _BillingCounty = value.County;
                _BillingCountry = value.Country;
                _BillingPostcode = value.Postcode;
            }
        }
        [NonSerialized]
        private string _BillingNumber;
        [NonSerialized]
        private string _BillingAddress1;
        [NonSerialized]
        private string _BillingAddress2;
        [NonSerialized]
        private string _BillingCity;
        [NonSerialized]
        private string _BillingCounty;
        [NonSerialized]
        private string _BillingCountry;
        [NonSerialized]
        private string _BillingPostcode;

        [JsonIgnore]
        public bool UseBilling { get; set; }

        [NotMapped]
        public TAddress ShippingAddress
        {
            get
            {
                if (UseBilling)
                    return default(TAddress);
                return new TAddress()
                {
                    Number = _ShippingNumber,
                    Address1 = _ShippingAddress1,
                    Address2 = _ShippingAddress2,
                    City = _ShippingCity,
                    County = _ShippingCounty,
                    Country = _ShippingCountry,
                    Postcode = _ShippingPostcode,
                };
            }
            set
            {
                _ShippingNumber = value.Number;
                _ShippingAddress1 = value.Address1;
                _ShippingAddress2 = value.Address2;
                _ShippingCity = value.City;
                _ShippingCounty = value.County;
                _ShippingCountry = value.Country;
                _ShippingPostcode = value.Postcode;
            }
        }

        [NonSerialized]
        private string _ShippingNumber;
        [NonSerialized]
        private string _ShippingAddress1;
        [NonSerialized]
        private string _ShippingAddress2;
        [NonSerialized]
        private string _ShippingCity;
        [NonSerialized]
        private string _ShippingCounty;
        [NonSerialized]
        private string _ShippingCountry;
        [NonSerialized]
        private string _ShippingPostcode;

    }
}
