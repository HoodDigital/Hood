using Hood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
