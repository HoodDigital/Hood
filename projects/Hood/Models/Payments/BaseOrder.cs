using Hood.Entities;
using Hood.Interfaces;
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
        private string _BillingNumber;
        private string _BillingAddress1;
        private string _BillingAddress2;
        private string _BillingCity;
        private string _BillingCounty;
        private string _BillingCountry;
        private string _BillingPostcode;

        [NotMapped]
        public TAddress ShippingAddress
        {
            get
            {
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
        private string _ShippingNumber;
        private string _ShippingAddress1;
        private string _ShippingAddress2;
        private string _ShippingCity;
        private string _ShippingCounty;
        private string _ShippingCountry;
        private string _ShippingPostcode;

    }
}
