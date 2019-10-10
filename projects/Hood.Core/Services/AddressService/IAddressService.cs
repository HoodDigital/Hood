using Geocoding.Google;
using Hood.Interfaces;

namespace Hood.Services
{
    public interface IAddressService
    {
        GoogleAddress GeocodeAddress(IAddress address);    
    }
}
