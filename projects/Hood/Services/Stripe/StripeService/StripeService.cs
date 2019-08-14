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
                return new Stripe.PlanService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.SubscriptionService SubscriptionService
        {
            get
            {
                return new Stripe.SubscriptionService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.CustomerService CustomerService
        {
            get
            {
                return new Stripe.CustomerService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.CardService CardService
        {
            get
            {
                return new Stripe.CardService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.ChargeService ChargeService
        {
            get
            {
                return new Stripe.ChargeService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.InvoiceService InvoiceService
        {
            get
            {
                return new Stripe.InvoiceService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.InvoiceItemService InvoiceItemService
        {
            get
            {
                return new Stripe.InvoiceItemService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.TokenService TokenService
        {
            get
            {
                return new Stripe.TokenService(new StripeClient(StripeApiKey));
            }
        }

        public Stripe.RefundService RefundService
        {
            get
            {
                return new Stripe.RefundService(new StripeClient(StripeApiKey));
            }
        }
    }
}