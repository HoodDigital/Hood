using System.Collections.Generic;
using System.Linq;
using Hood.Interfaces;
using Geocoding;
using Geocoding.Google;
using Hood.Extensions;

namespace Hood.Services
{
    public class AddressService : IAddressService
    {
        private readonly ISettingsRepository _settings;

        public AddressService(ISettingsRepository site)
        {
            _settings = site;
        }

        public Location GeocodeAddress(IAddress address)
        {
            var key = _settings.GetIntegrationSettings().GoogleMapsApiKey;
            if (!key.IsSet() || !_settings.GetIntegrationSettings().EnableGoogleGeocoding)
                return null;

            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = key };
            IEnumerable<Address> addresses = geocoder.Geocode(
                address.Number.IsSet() ? string.Format("{0} {1}", address.Number, address.Address1) : address.Address1, 
                address.City, 
                address.County, 
                address.Postcode, 
                address.Country
            );
            if (addresses.Count() == 0)
            {
                addresses = geocoder.Geocode(address.Postcode);
                if (addresses.Count() == 0)
                    return null;
            }

            return addresses.First().Coordinates;           
        }

    }
}
