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

        public StripePlanService PlanService
        {
            get
            {
                return new StripePlanService(StripeApiKey);
            }
        }

        public StripeSubscriptionService SubscriptionService
        {
            get
            {
                return new StripeSubscriptionService(StripeApiKey);
            }
        }

        public StripeCustomerService CustomerService
        {
            get
            {
                return new StripeCustomerService(StripeApiKey);
            }
        }

        public StripeCardService CardService
        {
            get
            {
                return new StripeCardService(StripeApiKey);
            }
        }

        public StripeChargeService ChargeService
        {
            get
            {
                return new StripeChargeService(StripeApiKey);
            }
        }

        public StripeInvoiceService InvoiceService
        {
            get
            {
                return new StripeInvoiceService(StripeApiKey);
            }
        }

        public StripeInvoiceItemService InvoiceItemService
        {
            get
            {
                return new StripeInvoiceItemService(StripeApiKey);
            }
        }

        public StripeTokenService TokenService
        {
            get
            {
                return new StripeTokenService(StripeApiKey);
            }
        }

        public StripeRefundService RefundService
        {
            get
            {
                return new StripeRefundService(StripeApiKey);
            }
        }
    }
}