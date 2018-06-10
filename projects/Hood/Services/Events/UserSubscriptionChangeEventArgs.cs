using Hood.Models;
using System;

namespace Hood.Services
{
    public class UserSubscriptionChangeEventArgs : EventArgs
    {
        public string Action { get; set; }
        public UserSubscription Subscription { get; set; }

        public UserSubscriptionChangeEventArgs(string changeAction, UserSubscription subsctription)
        {
            Action = changeAction;
            Subscription = subsctription;
        }
    }
}
