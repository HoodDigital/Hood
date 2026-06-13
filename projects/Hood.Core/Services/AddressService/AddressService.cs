using System;
using System.Collections.Generic;
using System.Linq;
using Geocoding;
using Geocoding.Google;
using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;

namespace Hood.Services
{
    public class AddressService : IAddressService
    {
        public GoogleAddress GeocodeAddress(IAddress address)
        {
            var key = Engine.Settings.Integrations.GoogleCloudApiKey;
            if (!key.IsSet() || !Engine.Settings.Integrations.EnableGoogleGeocoding)
                return null;

            // Geocoding must never surface as a 500 — an invalid/denied key, an over-quota
            // account or a transient HTTP failure all throw out of GoogleGeocoder (and the
            // blocking .Result rethrows them wrapped in an AggregateException, which is why
            // catching GoogleGeocodingException at the call sites was insufficient). Swallow
            // everything here, log it, and return null so callers fall back gracefully.
            try
            {
                IGeocoder geocoder = new GoogleGeocoder() { ApiKey = key };
                // Materialise the geocoder results — re-enumerating a lazy result here could
                // re-issue the HTTP geocode request.
                List<Address> addresses = geocoder
                    .GeocodeAsync(
                        address.Number.IsSet()
                            ? string.Format("{0} {1}", address.Number, address.Address1)
                            : address.Address1,
                        address.City,
                        address.County,
                        address.Postcode,
                        address.Country
                    )
                    .Result.ToList();
                if (addresses.Count == 0)
                {
                    addresses = geocoder.GeocodeAsync(address.Postcode).Result.ToList();
                    if (addresses.Count == 0)
                        return null;
                }

                return (GoogleAddress)addresses.First();
            }
            catch (Exception ex)
            {
                // Best-effort log; never let a logging failure mask the silent geocode fallback.
                try
                {
                    Engine
                        .Logs.AddExceptionAsync<AddressService>(
                            "Geocoding failed — check the Google Cloud API key has the Geocoding API enabled and is within quota. Returning no location.",
                            ex
                        )
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"AddressService: failed to log a geocoding error: {logEx.Message}"
                    );
                }

                return null;
            }
        }
    }
}
