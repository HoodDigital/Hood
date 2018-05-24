using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public partial class ApplicationUser : IdentityUser, ISaveableModel, IUserProfile
    {
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Display name")]
        public string DisplayName { get; set; }

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

        public bool Anonymous { get; set; }

        public string Bio { get; set; }

        public string ReplacePlaceholders(string msg)
        {
            return msg.Replace("{USERNAME}", UserName).Replace("{USEREMAIL}", Email).Replace("{FULLNAME}", FullName).Replace("{FIRSTNAME}", FirstName).Replace("{LASTNAME}", LastName);
        }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; }

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

        [Display(Name = "Forum Signature")]
        public string ForumSignature { get; set; }

        public string StripeId { get; set; }

        // Login logs and times
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string AvatarJson { get; set; }

        [NotMapped]
        public IMediaObject Avatar
        {
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(AvatarJson) : MediaObject.Blank; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }

        public string BillingAddressJson { get; set; }
        public string DeliveryAddressJson { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                if (Anonymous)
                    return "Anonymous";
                if (DisplayName.IsSet())
                    return DisplayName;
                if (FirstName.IsSet() && LastName.IsSet())
                    return FirstName + " " + LastName;
                else if (FirstName.IsSet() && !LastName.IsSet())
                    return FirstName;
                else if (!FirstName.IsSet() && LastName.IsSet())
                    return LastName;
                else return UserName;
            }
        }

        public List<Address> Addresses { get; set; }

        [NotMapped]
        public Address DeliveryAddress
        {
            get { return DeliveryAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(DeliveryAddressJson) : null; }
            set { DeliveryAddressJson = JsonConvert.SerializeObject(value); }
        }

        [NotMapped]
        public Address BillingAddress
        {
            get { return BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(BillingAddressJson) : null; }
            set { BillingAddressJson = JsonConvert.SerializeObject(value); }
        }

        public List<UserAccessCode> AccessCodes { get; set; }
        public List<Content> Content { get; set; }
        public List<Forum> Forums { get; set; }
        public List<Topic> Topics { get; set; }
        public List<PropertyListing> Properties { get; set; }
        public List<UserSubscription> Subscriptions { get; set; }

        public List<Post> Posts { get; set; }
        public List<Post> EditedPosts { get; set; }
        public List<Post> DeletedPosts { get; set; }

        [NotMapped]
        public AlertType MessageType { get; set; }
        [NotMapped]
        public string SaveMessage { get; set; }

        public void SetProfile(IUserProfile profile)
        {
            profile.CopyProperties(this);
        }

        public bool HasActiveSubscription(string category)
        {
            var subs = Subscriptions;
            if (category.IsSet())
                subs = subs.Where(s => s.Subscription.Category == category).ToList();
            foreach (var sub in subs)
                if (sub.IsActive)
                    return true;
            return false;
        }
        public UserSubscription GetActiveSubscription(string category)
        {
            var subs = Subscriptions;
            if (category.IsSet())
                subs = subs.Where(s => s.Subscription.Category == category).ToList();
            foreach (var sub in subs)
                if (sub.IsActive)
                    return sub;
            return null;
        }
        public bool IsSubscribed(int subscriptionId, bool requireActiveOrTrial = true)
        {
            foreach (var sub in Subscriptions)
            {
                if (sub.SubscriptionId == subscriptionId)
                {
                    if (requireActiveOrTrial && sub.IsActive)
                        return true;
                    if (!requireActiveOrTrial)
                        return true;
                }
            }
            return false;
        }
        public UserSubscription GetSubscription(int subscriptionId, bool requireActiveOrTrial = true)
        {
            foreach (var sub in Subscriptions)
            {
                if (sub.SubscriptionId == subscriptionId)
                {
                    if (requireActiveOrTrial && sub.IsActive)
                        return sub;
                    if (!requireActiveOrTrial)
                        return sub;
                }
            }
            return null;
        }
    }
}
