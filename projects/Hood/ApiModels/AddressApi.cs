using Hood.Extensions;

namespace Hood.Models.Api
{
    public partial class AddressApi
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

        public AddressApi(Address address, ApplicationUser user = null)
        {
            if (address == null)
                return;

            address.CopyProperties(this);

            // Formatted Members
            this.FullAddress = this.Address1 + ", ";
            if (!string.IsNullOrEmpty(this.Address2))
                this.FullAddress += this.Address2 + ", ";
            this.FullAddress += this.City + ", ";
            this.FullAddress += this.County + ", ";
            this.FullAddress += this.Country + ", ";
            this.FullAddress += this.Postcode;

            if (string.IsNullOrEmpty(this.QuickName))
                this.QuickName = this.Address1;

            if (user != null)
            {
                this.IsBilling = this.Id == user.BillingAddress?.Id;
                this.IsDelivery = this.Id == user.DeliveryAddress?.Id;
            }

        }

    }
}
