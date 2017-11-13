using Hood.Models;
using System;

namespace Hood.Services
{
    public class UserSubscriptionChangeEventArgs : EventArgs
    {
        public UserSubscription Subscription { get; set; }

        public UserSubscriptionChangeEventArgs(UserSubscription subsctription)
        {
            Subscription = subsctription;
        }
    }
}
