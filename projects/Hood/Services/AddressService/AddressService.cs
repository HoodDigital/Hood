using System.Collections.Generic;
using System.Linq;
using Hood.Interfaces;
using Geocoding;
using Geocoding.Google;
using Hood.Extensions;
using Hood.Core;

namespace Hood.Services
{
    public class AddressService : IAddressService
    {

        public AddressService()
        {
        }

        public GoogleAddress GeocodeAddress(IAddress address)
        {
            var key = Engine.Settings.Integrations.GoogleMapsApiKey;
            if (!key.IsSet() || !Engine.Settings.Integrations.EnableGoogleGeocoding)
                return null;

            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = key };
            IEnumerable<Address> addresses = geocoder.GeocodeAsync(
                address.Number.IsSet() ? string.Format("{0} {1}", address.Number, address.Address1) : address.Address1,
                address.City,
                address.County,
                address.Postcode,
                address.Country
            ).Result;
            if (addresses.Count() == 0)
            {
                addresses = geocoder.GeocodeAsync(address.Postcode).Result;
                if (addresses.Count() == 0)
                    return null;
            }

            return (GoogleAddress)addresses.First();
        }

    }
}
