using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stripe;
using Hood.Enums;
using Hood.BaseTypes;

namespace Hood.Models
{
    public class SubscriptionModel : SaveableModel
    {
        [Display(Name = "Stripe Card Token")]
        public string StripeToken { get; set; }

        [Display(Name = "Plan Id")]
        public int PlanId { get; set; }

        [Display(Name = "Exisiting CardId")]
        public string CardId { get; set; }

        [Display(Name = "Return Url")]
        public string returnUrl { get; set; }

        [Display(Name = "Current Category")]
        public string Category { get; set; }

        public List<Subscription> Plans { get; set; }
        public List<Subscription> Addons { get; set; }
        public StripeSubscription CurrentSubscription { get; set; }
        public StripeCustomer Customer { get; set; }
        public ApplicationUser User { get; set; }
        public IEnumerable<StripeCard> Cards { get; set; }
        public Subscription CurrentPlan { get; set; }
    }
}
