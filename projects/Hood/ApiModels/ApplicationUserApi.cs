using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hood.Models.Api
{
    public class ApplicationUserApi : ApplicationUserApi<HoodIdentityUser>
    {
        public ApplicationUserApi(IHoodUser user, ISettingsRepository settings = null)
            : base(user, settings)
        {
            if (user.Avatar != null)
                Avatar = new MediaApi(user.Avatar, settings);
            else
                Avatar = MediaApi.Blank(mediaSettings);

            Addresses = user.Addresses?.Select(s => new AddressApi<IHoodUser>(s)).ToList();
            DeliveryAddress = user.DeliveryAddress == null ? null : new AddressApi<IHoodUser>(user.DeliveryAddress);
            BillingAddress = user.BillingAddress == null ? null : new AddressApi<IHoodUser>(user.BillingAddress);

            Subscriptions = user.Subscriptions?.Select(s => new UserSubscriptionApi<IHoodUser>(s)).ToList();

        }
    }

    public class ApplicationUserApi<TUser> where TUser : IHoodUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Last name")]
        public string CompanyName { get; set; }

        [Display(Name = "Subscribe to newsletter?")]
        public bool EmailOptin { get; set; }

        public string Phone { get; set; }

        public string Mobile { get; set; }

        public string Twitter { get; set; }

        public string Facebook { get; set; }

        [Display(Name = "Google+")]
        public string GooglePlus { get; set; }

        public string LinkedIn { get; set; }

        public string Bio { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; }

        [Display(Name = "VAT Number")]
        public string VATNumber { get; set; }

        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }
        public string UserVars { get; set; }
        public string Notes { get; set; }
        public string SystemNotes { get; set; }

        // Login logs and times
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string FullName { get; set; }

        public int? AvatarId { get; set; }
        public int? AddressId { get; set; }
        public int? DeliveryAddressId { get; set; }
        public int? BillingAddressId { get; set; }

        public List<AddressApi<IHoodUser>> Addresses { get; set; }
        public AddressApi<IHoodUser> Address { get; set; }
        public AddressApi<IHoodUser> DeliveryAddress { get; set; }
        public AddressApi<IHoodUser> BillingAddress { get; set; }
        public MediaApi Avatar { get; set; }

        public List<UserSubscriptionApi<IHoodUser>> Subscriptions { get; set; }

        public ApplicationUserApi()
        {
        }

        public ApplicationUserApi(IHoodUser user)
        {

            if (user == null)
                return;

            user.CopyProperties(this);

            if (string.IsNullOrEmpty(LastLoginLocation))
            {
                LastLoginLocation = "[No data]";
            }
            if (string.IsNullOrEmpty(LastLoginIP))
            {
                LastLoginIP = "[No data]";
            }
        }

    }

}
