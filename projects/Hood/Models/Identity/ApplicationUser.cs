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
    public partial class ApplicationUser : IdentityUser, IUserProfile
    {
        #region IName 
        public bool Anonymous { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Display name")]
        public string DisplayName { get; set; }
        #endregion

        #region Metadata

        public string UserVars { get; set; }
        [NotMapped]
        private Dictionary<string, string> UserVariables
        {
            get { return UserVars.IsSet() ? JsonConvert.DeserializeObject<Dictionary<string, string>>(UserVars) : new Dictionary<string, string>(); }
            set { UserVars = JsonConvert.SerializeObject(value); }
        }
        public string this[string key]
        {
            get
            {
                if (UserVariables.ContainsKey(key))
                    return UserVariables[key];
                return null;
            }
            set
            {
                if (UserVariables.ContainsKey(key))
                    UserVariables[key] = value;
                else
                    UserVariables.Add(key, value);
            }
        }

        #endregion

        #region Notes 
        [NotMapped]
        public List<UserNote> Notes
        {
            get { return this[nameof(Notes)] != null ? JsonConvert.DeserializeObject<List<UserNote>>(this[nameof(Notes)]) : new List<UserNote>(); }
            set { this[nameof(Notes)] = JsonConvert.SerializeObject(value); }
        }
        public void AddUserNote(UserNote note)
        {
            var notes = this.Notes;
            notes.Add(note);
            this.Notes = notes;
        }
        #endregion

        #region LogOn Information
        [Display(Name = "Account Activated")]
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        #endregion

        #region Billing 

        [Display(Name = "Stripe Customer Id")]
        public string StripeId { get; set; }

        #endregion

        #region Avatar
        public string AvatarJson { get; set; }
        [NotMapped]
        public IMediaObject Avatar
        {
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(AvatarJson) : MediaObject.Blank; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Addresses 
        public List<Address> Addresses { get; set; }

        public string BillingAddressJson { get; set; }
        [NotMapped]
        public Address DeliveryAddress
        {
            get { return DeliveryAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(DeliveryAddressJson) : null; }
            set { DeliveryAddressJson = JsonConvert.SerializeObject(value); }
        }

        public string DeliveryAddressJson { get; set; }
        [NotMapped]
        public Address BillingAddress
        {
            get { return BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(BillingAddressJson) : null; }
            set { BillingAddressJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Socials 
        [NotMapped]
        [Display(Name = "Website URL")]
        public string WebsiteUrl { get => this[nameof(WebsiteUrl)]; set => this[nameof(WebsiteUrl)] = value; }
        [NotMapped]
        [Display(Name = "Twitter URL")]
        public string Twitter { get => this[nameof(Twitter)]; set => this[nameof(Twitter)] = value; }
        [NotMapped]
        [Display(Name = "Twitter Handle (@yourname)")]
        public string TwitterHandle { get => this[nameof(TwitterHandle)]; set => this[nameof(TwitterHandle)] = value; }
        [NotMapped]
        [Display(Name = "Facebook URL")]
        public string Facebook { get => this[nameof(Facebook)]; set => this[nameof(Facebook)] = value; }
        [NotMapped]
        [Display(Name = "Instagram URL")]
        public string Instagram { get => this[nameof(Instagram)]; set => this[nameof(Instagram)] = value; }
        [NotMapped]
        [Display(Name = "LinkedIn URL")]
        public string LinkedIn { get => this[nameof(LinkedIn)]; set => this[nameof(LinkedIn)] = value; }
        #endregion

        #region Extra Profile Fields
        [NotMapped]
        [Display(Name = "Forum Signature")]
        public string ForumSignature { get => this[nameof(ForumSignature)]; set => this[nameof(ForumSignature)] = value; }
        [NotMapped]
        [Display(Name = "Company name")]
        public string CompanyName { get => this[nameof(CompanyName)]; set => this[nameof(CompanyName)] = value; }
        [NotMapped]
        [Display(Name = "Bio")]
        public string Bio { get => this[nameof(Bio)]; set => this[nameof(Bio)] = value; }
        [NotMapped]
        [Display(Name = "Job Title")]
        public string JobTitle { get => this[nameof(JobTitle)]; set => this[nameof(JobTitle)] = value; }
        #endregion

        #region Related Tables
        [JsonIgnore]
        public List<UserAccessCode> AccessCodes { get; set; }
        [JsonIgnore]
        public List<ApiKey> ApiKeys { get; set; }
        [JsonIgnore]
        public List<Content> Content { get; set; }
        [JsonIgnore]
        public List<Forum> Forums { get; set; }
        [JsonIgnore]
        public List<Topic> Topics { get; set; }
        [JsonIgnore]
        public List<PropertyListing> Properties { get; set; }
        [JsonIgnore]
        public List<UserSubscription> Subscriptions { get; set; }
        [JsonIgnore]
        public List<Post> Posts { get; set; }
        [JsonIgnore]
        public List<Post> EditedPosts { get; set; }
        [JsonIgnore]
        public List<Post> DeletedPosts { get; set; }
        #endregion

        #region ISaveableModel
        [NotMapped]
        [JsonIgnore]
        public AlertType MessageType { get; set; }
        [NotMapped]
        [JsonIgnore]
        public string SaveMessage { get; set; }
        #endregion

        #region Profile Get/Set 
        [NotMapped]
        public IUserProfile Profile
        {
            get { return this as IUserProfile; }
            set { value.CopyProperties(this); }
        }
        #endregion

        #region Subscription Helpers
        public bool HasActiveSubscription(string category = null)
        {
            var subs = Subscriptions;
            if (category.IsSet())
                subs = subs.Where(s => s.Subscription.Category == category).ToList();
            foreach (var sub in subs)
                if (sub.IsActive)
                    return true;
            return false;
        }
        public UserSubscription GetActiveSubscription(string category = null)
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
        #endregion
    }
}
