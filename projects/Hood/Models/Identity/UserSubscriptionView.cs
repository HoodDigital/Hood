using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Hood.Models
{
    public class UserSubscriptionsView : UserProfile
    {
        #region Subscription Data
        public int ActiveCount { get; set; }
        public int TrialCount { get; set; }
        public int InactiveCount { get; set; }
        public int OverDueCount { get; set; }
        public int TotalSubscriptions { get; set; }

        internal string _Subscriptions { get; set; }
        public List<UserSubscriptionInfo> Subscriptions
        {
            get { return !_Subscriptions.IsSet() ? new List<UserSubscriptionInfo>() : JsonConvert.DeserializeObject<List<UserSubscriptionInfo>>(_Subscriptions); }
            set { _Subscriptions = JsonConvert.SerializeObject(value); }
        }

        internal ApplicationUser AsUser()
        {
            return (ApplicationUser)(IUserProfile)this;
        }
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
