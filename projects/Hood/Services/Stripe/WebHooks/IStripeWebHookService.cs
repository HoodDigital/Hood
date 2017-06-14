using Stripe;
using Hood.Extensions;

namespace Hood.Services
{
    public interface IStripeWebHookService
    {
        void ChargeCaptured(StripeEvent stripeEvent);
        void ChargeFailed(StripeEvent stripeEvent);
        void ChargeRefunded(StripeEvent stripeEvent);
        void ChargeSucceeded(StripeEvent stripeEvent);
        void ChargeUpdated(StripeEvent stripeEvent);
        void CustomerCardCreated(StripeEvent stripeEvent);
        void CustomerCardDeleted(StripeEvent stripeEvent);
        void CustomerCardUpdated(StripeEvent stripeEvent);
        void CustomerCreated(StripeEvent stripeEvent);
        void CustomerDeleted(StripeEvent stripeEvent);
        void CustomerUpdated(StripeEvent stripeEvent);
        void InvoiceCreated(StripeEvent stripeEvent);
        void InvoiceItemCreated(StripeEvent stripeEvent);
        void InvoiceItemDeleted(StripeEvent stripeEvent);
        void InvoiceItemUpdated(StripeEvent stripeEvent);
        void InvoicePaymentFailed(StripeEvent stripeEvent);
        void InvoicePaymentSucceeded(StripeEvent stripeEvent);
        void InvoiceUpdated(StripeEvent stripeEvent);
        void PlanCreated(StripeEvent stripeEvent);
        void PlanDeleted(StripeEvent stripeEvent);
        void PlanUpdated(StripeEvent stripeEvent);
        void ProcessEvent(string json);
        void SubscriptionCreated(StripeEvent stripeEvent);
        void SubscriptionDeleted(StripeEvent stripeEvent);
        void SubscriptionTrialWillEnd(StripeEvent stripeEvent);
        void SubscriptionUpdated(StripeEvent stripeEvent);
        void UnhandledWebHook(StripeEvent stripeEvent);
    }
    public static class IStripeWebHookExtensions
    {
        public static void ProcessEventByType(this IStripeWebHookService service, StripeEvent stripeEvent)
        {
            switch (stripeEvent.GetEventName())
            {
                case "charge.succeeded":
                    service.ChargeSucceeded(stripeEvent);
                    break;
                case "charge.failed":
                    service.ChargeFailed(stripeEvent);
                    break;
                case "charge.refunded":
                    service.ChargeRefunded(stripeEvent);
                    break;
                case "charge.captured":
                    service.ChargeCaptured(stripeEvent);
                    break;
                case "charge.updated":
                    service.ChargeUpdated(stripeEvent);
                    break;
                case "customer.created":
                    service.CustomerCreated(stripeEvent);
                    break;
                case "customer.updated":
                    service.CustomerUpdated(stripeEvent);
                    break;
                case "customer.deleted":
                    service.CustomerDeleted(stripeEvent);
                    break;
                case "customer.card.created":
                    service.CustomerCardCreated(stripeEvent);
                    break;
                case "customer.card.updated":
                    service.CustomerCardUpdated(stripeEvent);
                    break;
                case "customer.card.deleted":
                    service.CustomerCardDeleted(stripeEvent);
                    break;
                case "customer.subscription.created":
                    service.SubscriptionCreated(stripeEvent);
                    break;
                case "customer.subscription.updated":
                    service.SubscriptionUpdated(stripeEvent);
                    break;
                case "customer.subscription.deleted":
                    service.SubscriptionDeleted(stripeEvent);
                    break;
                case "customer.subscription.trial_will_end":
                    service.SubscriptionTrialWillEnd(stripeEvent);
                    break;
                case "invoice.created":
                    service.InvoiceCreated(stripeEvent);
                    break;
                case "invoice.payment_failed":
                    service.InvoicePaymentFailed(stripeEvent);
                    break;
                case "invoice.payment_succeeded":
                    service.InvoicePaymentSucceeded(stripeEvent);
                    break;
                case "invoice.updated":
                    service.InvoiceUpdated(stripeEvent);
                    break;
                case "invoiceitem.created":
                    service.InvoiceItemCreated(stripeEvent);
                    break;
                case "invoiceitem.updated":
                    service.InvoiceItemUpdated(stripeEvent);
                    break;
                case "invoiceitem.deleted":
                    service.InvoiceItemDeleted(stripeEvent);
                    break;
                case "plan.created":
                    service.PlanCreated(stripeEvent);
                    break;
                case "plan.updated":
                    service.PlanUpdated(stripeEvent);
                    break;
                case "plan.deleted":
                    service.PlanDeleted(stripeEvent);
                    break;
                default:
                    service.UnhandledWebHook(stripeEvent);
                    break;
            }
        }

    }
}