using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.BaseTypes;

namespace Hood.ViewModels
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

        public List<Models.Subscription> Plans { get; set; }
        public List<Models.Subscription> Addons { get; set; }
        public Stripe.Subscription CurrentSubscription { get; set; }
        public Stripe.Customer Customer { get; set; }
        public IEnumerable<Stripe.Card> Cards { get; set; }
        public Models.Subscription CurrentPlan { get; set; }
    }
}
