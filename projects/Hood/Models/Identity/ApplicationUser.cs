using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Hood.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public partial class ApplicationUser : IdentityUser
    {       
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Display name")]
        public string DisplayName { get; set; }

        [Display(Name = "Subscribe to newsletter?")]
        public bool EmailOptin { get; set; }

        public string Phone { get; set; }

        public string Mobile { get; set; }

        [Display(Name = "Twitter URL")]
        public string Twitter { get; set; }

        [Display(Name = "Twitter Handle (@yourname)")]
        public string TwitterHandle { get; set; }

        [Display(Name = "Facebook URL")]
        public string Facebook { get; set; }

        [Display(Name = "Google+ URL")]
        public string GooglePlus { get; set; }

        [Display(Name = "LinkedIn URL")]
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
        [NotMapped]
        public Dictionary<string, string> UserVariables
        {
            get { return UserVars.IsSet() ? JsonConvert.DeserializeObject<Dictionary<string, string>>(UserVars) : new Dictionary<string, string>(); }
            set { UserVars = JsonConvert.SerializeObject(value); }
        }

        [Display(Name = "Account Activated")]
        public bool Active { get; set; }

        public string Notes { get; set; }
        public string SystemNotes { get; set; }

        public string StripeId { get; set; }

        // Login logs and times
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public List<Address> Addresses { get; set; }

        public string DeliveryAddressJson { get; set; }
        [NotMapped]
        public Address DeliveryAddress
        {
            get { return DeliveryAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(DeliveryAddressJson) : null; }
            set { DeliveryAddressJson = JsonConvert.SerializeObject(value); }
        }

        public string BillingAddressJson { get; set; }
        [NotMapped]
        public Address BillingAddress
        {
            get { return BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(BillingAddressJson) : null; }
            set { BillingAddressJson = JsonConvert.SerializeObject(value); }
        }

        public string AvatarJson { get; set; }
        [NotMapped]
        public SiteMedia Avatar
        {
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<SiteMedia>(AvatarJson) : null; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }

        public List<UserAccessCode> AccessCodes { get; set; }
        public List<Content> Content { get; set; }
        public List<PropertyListing> Properties { get; set; }
        public List<UserSubscription> Subscriptions { get; set; }

        public string FullName {
            get
            {
                if (FirstName.IsSet() && LastName.IsSet())
                    return FirstName + " " + LastName;
                else if (FirstName.IsSet() && !LastName.IsSet())
                    return FirstName;
                else if (!FirstName.IsSet() && LastName.IsSet())
                    return LastName;
                else return UserName;
            }
        }
    }
}
