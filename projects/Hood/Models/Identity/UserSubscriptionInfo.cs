using System;

namespace Hood.Models
{
    public class UserSubscriptionInfo
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string StripeId { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool Public { get; set; }
        public int Level { get; set; }
        public int Amount { get; set; }
        public int SubscriptionProductId { get; set; }
        public bool Addon { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; }

    }
}