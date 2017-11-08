using Hood.Extensions;
using Hood.Interfaces;

namespace Hood.Models.Api
{
    public class AddressApi : AddressApi<HoodIdentityUser>
    {
        public AddressApi(IAddress address, IHoodUser user = null)
            : base(address, user)
        {
        }
    }

    public partial class AddressApi<TUser> where TUser : IHoodUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string QuickName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        // Formatted Members
        public string FullAddress { get; set; }
        public bool IsHome { get; set; }
        public bool IsDelivery { get; set; }
        public bool IsBilling { get; set; }

        public AddressApi(IAddress address, IHoodUser user = null)
            
        {
            if (address == null)
                return;

            address.CopyProperties(this);

            // Formatted Members
            FullAddress = Address1 + ", ";
            if (!string.IsNullOrEmpty(Address2))
                FullAddress += Address2 + ", ";
            FullAddress += City + ", ";
            FullAddress += County + ", ";
            FullAddress += Country + ", ";
            FullAddress += Postcode;

            if (string.IsNullOrEmpty(QuickName))
                QuickName = Address1;

            if (user != null)
            {
                IsBilling = Id == user.BillingAddress?.Id;
                IsDelivery = Id == user.DeliveryAddress?.Id;
            }

        }

    }
}
