using Hood.Models.Api;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class AccountInfo
    {
        public IList<string> Roles { get; set; }
        public ApplicationUser User { get; set; }
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
}