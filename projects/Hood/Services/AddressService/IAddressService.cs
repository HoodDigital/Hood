using Geocoding;
using Hood.Interfaces;

namespace Hood.Services
{
    public interface IAddressService
    {
        Location GeocodeAddress(IAddress address);    
    }
}
