using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public class UserProfile : UserProfileBase
    {
        #region Subscription Data
        public int ActiveCount { get; set; }
        public int TrialCount { get; set; }
        public int InactiveCount { get; set; }
        public int OverDueCount { get; set; }
        public int TotalSubscriptions { get; set; }
        public string ActiveSubscriptionIds { get; set; }
        public string RoleIds { get; set; }

        internal string SubscriptionsJson { get; set; }
        public List<UserSubscriptionInfo> Subscriptions
        {
            get { return !SubscriptionsJson.IsSet() ? new List<UserSubscriptionInfo>() : JsonConvert.DeserializeObject<List<UserSubscriptionInfo>>(SubscriptionsJson); }
            set { SubscriptionsJson = JsonConvert.SerializeObject(value); }
        }
        public List<UserSubscriptionInfo> ActiveSubscriptions
        {
            get
            {
                return Subscriptions.Where(s => s.Status == "active" || s.Status == "trialing").ToList();
            }
        }

        public int RoleCount { get; set; }
        internal string RolesJson { get; set; }
        public List<IdentityRole> Roles
        {
            get { return !RolesJson.IsSet() ? new List<IdentityRole>() : JsonConvert.DeserializeObject<List<IdentityRole>>(RolesJson); }
            set { RolesJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region View Model Stuff
        [NotMapped]
        public IList<IdentityRole> AllRoles { get; set; }
        [NotMapped]
        public Customer Customer { get; set; }
        [NotMapped]
        public List<Customer> MatchedCustomerObjects { get; set; }
        #endregion

        #region Prevent Mapping Sensitive Fields
        [NotMapped]
        public override DateTimeOffset? LockoutEnd { get; set; }
        [NotMapped]
        public override string ConcurrencyStamp { get; set; }
        [NotMapped]
        public override string SecurityStamp { get; set; }
        [NotMapped]
        public override string PasswordHash { get; set; }
        [NotMapped]
        public override string NormalizedEmail { get; set; }
        [NotMapped]
        public override string NormalizedUserName { get; set; }
        [NotMapped]
        public override int AccessFailedCount { get; set; }
        #endregion
    }
}
