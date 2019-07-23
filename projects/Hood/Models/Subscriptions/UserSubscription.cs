using Hood.Entities;
using Microsoft.AspNetCore.Html;
using System;

namespace Hood.Models
{
    public partial class UserSubscription : BaseEntity
    {
        public bool Confirmed { get; set; }
        public bool Deleted { get; set; }
        public string StripeId { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public string CustomerId { get; set; }
        public DateTime? EndedAt { get; set; }
        public long Quantity { get; set; }
        public DateTime? Start { get; set; }
        public string Status { get; set; }
        public IHtmlContent StatusBadge
        {
            get
            {
                switch (Status)
                {
                    case Stripe.SubscriptionStatuses.Active:
                        return new HtmlString("<span class='badge badge-success'><i class='fa fa-check-circle mr-2'></i>Active</span>");
                    case Stripe.SubscriptionStatuses.Trialing:
                        return new HtmlString("<span class='badge badge-warning'><i class='fa fa-info mr-2'></i>Trialing</span>");
                    case Stripe.SubscriptionStatuses.Canceled:
                        return new HtmlString("<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>Canceled</span>");
                    case Stripe.SubscriptionStatuses.PastDue:
                        return new HtmlString("<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>PastDue</span>");
                    case Stripe.SubscriptionStatuses.Incomplete:
                        return new HtmlString("<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>Incomplete</span>");
                    case Stripe.SubscriptionStatuses.IncompleteExpired:
                        return new HtmlString("<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>Incomplete Expired</span>");
                    case Stripe.SubscriptionStatuses.Unpaid:
                        return new HtmlString("<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>Unpaid</span>");
                }
                return new HtmlString($"<span class='badge badge-danger'><i class='fa fa-exclamation-triangle mr-2'></i>{Status}</span>");
            }
        }

        public decimal? TaxPercent { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? TrialStart { get; set; }
        public string Notes { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime DeletedAt { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        public bool Tiered => Subscription != null ? !Subscription.Addon : false;
        public int Level => Subscription != null ? Subscription.Level : 0;

        public bool IsActive => Status == "trialing" || Status == "active";


    }

    public static class UserSubscriptionExtensions
    {
        public static UserSubscription UpdateFromStripe(this UserSubscription userSubscription, Stripe.Subscription stripeSubscription)
        {
            userSubscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
            userSubscription.CanceledAt = stripeSubscription.CanceledAt;
            userSubscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
            userSubscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
            userSubscription.EndedAt = stripeSubscription.EndedAt;
            userSubscription.Quantity = stripeSubscription.Quantity ?? 0;
            userSubscription.Start = stripeSubscription.StartDate;
            userSubscription.Status = stripeSubscription.Status;
            userSubscription.TrialEnd = stripeSubscription.TrialEnd;
            userSubscription.TrialStart = stripeSubscription.TrialStart;
            userSubscription.Notes += DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + " Stripe.Event - Updated Subscription" + Environment.NewLine;
            userSubscription.LastUpdated = DateTime.Now;
            return userSubscription;
        }

    }
}
