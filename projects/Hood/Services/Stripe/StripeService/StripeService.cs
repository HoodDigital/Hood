using Stripe;
using Hood.Models;

namespace Hood.Services
{
    public class StripeService : IStripeService
    {
        string StripeApiKey { get; set; }
        public StripeService(ISettingsRepository site)
        {
            BillingSettings settings = site.GetBillingSettings();
            if (settings.EnableStripeTestMode)
                StripeApiKey = settings.StripeTestKey;
            else
                StripeApiKey = settings.StripeLiveKey;
        }

        public Stripe.PlanService PlanService
        {
            get
            {
                return new Stripe.PlanService(StripeApiKey);
            }
        }

        public Stripe.SubscriptionService SubscriptionService
        {
            get
            {
                return new Stripe.SubscriptionService(StripeApiKey);
            }
        }

        public Stripe.CustomerService CustomerService
        {
            get
            {
                return new Stripe.CustomerService(StripeApiKey);
            }
        }

        public Stripe.CardService CardService
        {
            get
            {
                return new Stripe.CardService(StripeApiKey);
            }
        }

        public Stripe.ChargeService ChargeService
        {
            get
            {
                return new Stripe.ChargeService(StripeApiKey);
            }
        }

        public Stripe.InvoiceService InvoiceService
        {
            get
            {
                return new Stripe.InvoiceService(StripeApiKey);
            }
        }

        public Stripe.InvoiceItemService InvoiceItemService
        {
            get
            {
                return new Stripe.InvoiceItemService(StripeApiKey);
            }
        }

        public Stripe.TokenService TokenService
        {
            get
            {
                return new Stripe.TokenService(StripeApiKey);
            }
        }

        public Stripe.RefundService RefundService
        {
            get
            {
                return new Stripe.RefundService(StripeApiKey);
            }
        }
    }
}