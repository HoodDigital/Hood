using Hood.Models.Api;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public partial class AccountInfo
    {
        public IList<string> Roles { get; set; }
        public HoodIdentityUser User { get; set; }
        public List<UserSubscriptionApi> ActiveSubscriptions { get; set; }

        public bool IsSubscribed(string id)
        {
            return ActiveSubscriptions.Select(a => a.StripeId).Contains(id);
        }
        public bool Subscribed
        {
            get
            {
                return ActiveSubscriptions.Count > 0;
            }
        }
        public bool HasTieredSubscription
        {
            get
            {
                return ActiveSubscriptions.Where(s => s.Tiered).Count() > 0;
            }
        }
        public AccountInfo()
        {
            ActiveSubscriptions = new List<UserSubscriptionApi>();
            Roles = new List<string>();
        }
    }
    public class AccountInfo<TUser> where TUser : IHoodUser
    {
        public IList<string> Roles { get; set; }
        public TUser User { get; set; }
        public List<UserSubscriptionApi<TUser>> ActiveSubscriptions { get; set; }

        public bool IsSubscribed(string id)
        {
            return ActiveSubscriptions.Select(a => a.StripeId).Contains(id);
        }
        public bool Subscribed
        {
            get
            {
                return ActiveSubscriptions.Count > 0;
            }
        }
        public bool HasTieredSubscription
        {
            get
            {
                return ActiveSubscriptions.Where(s => s.Tiered).Count() > 0;
            }
        }
        public AccountInfo()
        {
            ActiveSubscriptions = new List<UserSubscriptionApi<TUser>>();
            Roles = new List<string>();
        }
    }
}