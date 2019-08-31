using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.BaseTypes;
using Hood.Models;
using Stripe;

namespace Hood.ViewModels
{
    public class SubscriptionModelBase : SaveableModel
    {
        [Display(Name = "Return Url")]
        public string ReturnUrl { get; set; }
        public Stripe.Customer Customer { get; set; }
    }
    public class SubscriptionProductsModel : SubscriptionModelBase
    {
      public List<SubscriptionProduct> Products { get; set; }
    }
    public class SubscriptionPlansModel : SubscriptionModelBase
    {
        public SubscriptionProduct Product { get; set; }
        public List<SubscriptionPlan> Plans { get; set; }
    }
    public class BuySubscriptionModel : SubscriptionModelBase
    {
        public Models.Subscription Plan { get; set; }
        public string ClientSecret { get; set; }
    }
    public class SubscriptionWelcomeModel : SubscriptionModelBase
    {
        public UserSubscription CurrentUserSubscription { get; set; }
        public Stripe.Subscription CurrentSubscription { get; set; }
    }
    public class ConfirmSubscriptionModel : SubscriptionModelBase
    {
        public int PlanId { get; set; }
        public string Token { get; set; }
        public string PaymentMethodId { get; set; }
        public string IntentId { get; set; }
    }
    public class ChangeSubscriptionModel : SubscriptionPlansModel
    {
        public int PlanId { get; set; }
        public Models.Subscription Plan { get; set; }
        public int SubscriptionId { get; set; }
        public UserSubscription CurrentSubscription { get; set; }
    }
}
