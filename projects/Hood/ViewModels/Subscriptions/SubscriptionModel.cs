using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stripe;
using Hood.Enums;

namespace Hood.Models
{
    public class SubscriptionModel<TUser> where TUser : IHoodUser
    {
        [Display(Name = "Stripe Card Token")]
        public string StripeToken { get; set; }

        [Display(Name = "Plan Id")]
        public int PlanId { get; set; }

        [Display(Name = "Exisiting CardId")]
        public string CardId { get; set; }       

        [Display(Name = "Return Url")]
        public string returnUrl { get; set; }

        public List<Subscription<TUser>> Plans { get; set; }
        public List<Subscription<TUser>> Addons { get; set; }
        public StripeSubscription CurrentSubscription { get; set; }
        public StripeCustomer Customer { get; set; }
        public TUser User { get; set; }
        public BillingMessage? Message { get; set; }
        public string MessageText { get; set; }
        public IEnumerable<StripeCard> Cards { get; set; }
        public Subscription<TUser> CurrentPlan { get; internal set; }
    }
}
