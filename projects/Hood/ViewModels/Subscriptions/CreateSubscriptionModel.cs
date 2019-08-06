using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class CreateSubscriptionModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Price",
             Description = "The price in your chosen currency for the subscription, per interval.")]
        public decimal Amount { get; set; }

        public string Currency { get; set; }

        [Display(Name = "Subscription Category",
              Description = "A user can only subscribe to one of each category. Think of categories as products, with different subscription levels.")]
        public string Category { get; set; }

        [Display(Name = "Charge Interval",
                Description = "The time period in which the subscription cycle is measured.")]
        public string Interval { get; set; }

        [Display(Name = "Interval Count",
                 Description = "How many intervals between charges.")]
        public int IntervalCount { get; set; }

        public bool LiveMode { get; set; }

        [Display(Name = "Code",
                 Description = "Used as a unique id for the subscription (leave blank to auto generate)")]
        public string StripeId { get; set; }

        [Display(Name = "Public",
                Description = "Display this as a public subscription (User can up/downgrade to it)")]
        public bool Public { get; set; }

        [Display(Name = "Add On",
                Description = "Addons are excluded from the level based subscriptions, and can be bolted onto accounts in addition to regular subscriptions.")]
        public bool AddOn { get; set; }

        [Display(Name = "Trial Period (Days)",
                 Description = "If entered, a trial will be active before charging for the set number of days.")]
        public int? TrialPeriodDays { get; set; }

        [Display(Name = "Level",
                 Description = "This is the level of the subscription, higher numbers indicate a higher access level, subscriptions are ordered by level.")]
        public int Level { get; set; }
    }
}