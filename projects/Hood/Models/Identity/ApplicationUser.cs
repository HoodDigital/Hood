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
    public partial class ApplicationUser : UserProfile
    {
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
