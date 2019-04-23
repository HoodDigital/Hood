using Stripe;
using Hood.Extensions;

namespace Hood.Services
{
    public interface IStripeWebHookService
    {
        void ChargeCaptured(Stripe.Event stripeEvent);
        void ChargeFailed(Stripe.Event stripeEvent);
        void ChargeRefunded(Stripe.Event stripeEvent);
        void ChargeSucceeded(Stripe.Event stripeEvent);
        void ChargeUpdated(Stripe.Event stripeEvent);
        void CustomerCardCreated(Stripe.Event stripeEvent);
        void CustomerCardDeleted(Stripe.Event stripeEvent);
        void CustomerCardUpdated(Stripe.Event stripeEvent);
        void CustomerCreated(Stripe.Event stripeEvent);
        void CustomerDeleted(Stripe.Event stripeEvent);
        void CustomerUpdated(Stripe.Event stripeEvent);
        void InvoiceCreated(Stripe.Event stripeEvent);
        void InvoiceItemCreated(Stripe.Event stripeEvent);
        void InvoiceItemDeleted(Stripe.Event stripeEvent);
        void InvoiceItemUpdated(Stripe.Event stripeEvent);
        void InvoicePaymentFailed(Stripe.Event stripeEvent);
        void InvoicePaymentSucceeded(Stripe.Event stripeEvent);
        void InvoiceUpdated(Stripe.Event stripeEvent);
        void PlanCreated(Stripe.Event stripeEvent);
        void PlanDeleted(Stripe.Event stripeEvent);
        void PlanUpdated(Stripe.Event stripeEvent);
        void ProcessEvent(string json);
        void SubscriptionCreated(Stripe.Event stripeEvent);
        void SubscriptionDeleted(Stripe.Event stripeEvent);
        void SubscriptionTrialWillEnd(Stripe.Event stripeEvent);
        void SubscriptionUpdated(Stripe.Event stripeEvent);
        void UnhandledWebHook(Stripe.Event stripeEvent);
    }
    public static class IStripeWebHookExtensions
    {
        public static void ProcessEventByType(this IStripeWebHookService service, Stripe.Event stripeEvent)
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