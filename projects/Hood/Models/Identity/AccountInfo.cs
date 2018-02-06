using Hood.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public partial class AccountInfo
    {
        public IList<string> Roles { get; set; }
        public ApplicationUser User { get; set; }
        public List<UserSubscription> ActiveSubscriptions { get; set; }

        public bool IsSubscribed(string id)
        {
            return ActiveSubscriptions.Select(a => a.StripeId).Contains(id);
        }
        public bool IsSubscribedToCategory(string category)
        {
            return ActiveSubscriptions.Select(a => a.Subscription.Category).Contains(category);
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
            ActiveSubscriptions = new List<UserSubscription>();
            Roles = new List<string>();
        }
    }
}