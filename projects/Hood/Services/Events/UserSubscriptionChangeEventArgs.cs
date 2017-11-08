using Hood.Models;
using System;

namespace Hood.Services
{
    public class UserSubscriptionChangeEventArgs : EventArgs
    {
        public UserSubscription<HoodIdentityUser> Subscription { get; set; }

        public UserSubscriptionChangeEventArgs(UserSubscription<HoodIdentityUser> subsctription)
        {
            Subscription = subsctription;
        }
    }
}
