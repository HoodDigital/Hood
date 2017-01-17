using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public partial class UserSubscription
    {
        [Key]
        public int UserSubscriptionId { get; set; }
        public bool Confirmed { get; set; }
        public bool Deleted { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        public string StripeId { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public string CustomerId { get; set; }
        public DateTime? EndedAt { get; set; }
        public int Quantity { get; set; }
        public DateTime? Start { get; set; }
        public string Status { get; set; }
        public decimal? TaxPercent { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? TrialStart { get; set; }
        public string Notes { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
