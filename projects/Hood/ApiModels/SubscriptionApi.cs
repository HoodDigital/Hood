using Hood.Extensions;
using System;
using System.Linq;

namespace Hood.Models.Api
{
    public class SubscriptionApi
    {
        public int Id { get; set; }
        public string StripeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Colour { get; set; }
        public bool Public { get; set; }
        public int Level { get; set; }

        public bool Addon { get; set; }
        public int NumberAllowed { get; set; }

        // Stripe Fields
        public int Amount { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; }
        public bool LiveMode { get; set; }
        public string StatementDescriptor { get; set; }
        public int? TrialPeriodDays { get; set; }
        public int SubscriberCount { get; set; }
        public int ActiveSubscribers { get; set; }
        public string FeaturedImageUrl { get; set; }

        // Creator/Editor
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }


        public string Price
        {
            get
            {
                return ((double)Amount / 100).ToString("C");
            }
        }
        public string FullPrice
        {
            get
            {
                return ((double)Amount / 100).ToString("C") + " every " + IntervalCount + " " + Interval + "(s)";
            }
        }

        public SubscriptionApi(Subscription sub)
        {
            if (sub == null)
                return;
            sub.CopyProperties(this);
            if (sub.Users == null)
            {
                SubscriberCount = 0;
                ActiveSubscribers = 0;
            }
            else
            {
                SubscriberCount = sub.Users.Count;
                ActiveSubscribers = sub.Users.Where(u => u.Status == "active" || u.Status == "trialing").Count();
            }

        }
    }
}