using System.Threading.Tasks;
using Stripe;
using Hood.Core.Extensions;

namespace Hood.Services
{
    public interface IStripeWebHookService
    {
        Task ChargeCaptured(StripeEvent stripeEvent);
        Task ChargeFailed(StripeEvent stripeEvent);
        Task ChargeRefunded(StripeEvent stripeEvent);
        Task ChargeSucceeded(StripeEvent stripeEvent);
        Task ChargeUpdated(StripeEvent stripeEvent);
        Task CustomerCardCreated(StripeEvent stripeEvent);
        Task CustomerCardDeleted(StripeEvent stripeEvent);
        Task CustomerCardUpdated(StripeEvent stripeEvent);
        Task CustomerCreated(StripeEvent stripeEvent);
        Task CustomerDeleted(StripeEvent stripeEvent);
        Task CustomerUpdated(StripeEvent stripeEvent);
        Task InvoiceCreated(StripeEvent stripeEvent);
        Task InvoiceItemCreated(StripeEvent stripeEvent);
        Task InvoiceItemDeleted(StripeEvent stripeEvent);
        Task InvoiceItemUpdated(StripeEvent stripeEvent);
        Task InvoicePaymentFailed(StripeEvent stripeEvent);
        Task InvoicePaymentSucceeded(StripeEvent stripeEvent);
        Task InvoiceUpdated(StripeEvent stripeEvent);
        Task PlanCreated(StripeEvent stripeEvent);
        Task PlanDeleted(StripeEvent stripeEvent);
        Task PlanUpdated(StripeEvent stripeEvent);
        Task ProcessEvent(string json);
        Task SubscriptionCreated(StripeEvent stripeEvent);
        Task SubscriptionDeleted(StripeEvent stripeEvent);
        Task SubscriptionTrialWillEnd(StripeEvent stripeEvent);
        Task SubscriptionUpdated(StripeEvent stripeEvent);
        Task UnhandledWebHook(StripeEvent stripeEvent);
    }
    public static class IStripeWebHookExtensions
    {
        public static async void ProcessEventByType(this IStripeWebHookService service, StripeEvent stripeEvent)
        {
            switch (stripeEvent.GetEventName())
            {
                case "charge.succeeded":
                    await service.ChargeSucceeded(stripeEvent);
                    break;
                case "charge.failed":
                    await service.ChargeFailed(stripeEvent);
                    break;
                case "charge.refunded":
                    await service.ChargeRefunded(stripeEvent);
                    break;
                case "charge.captured":
                    await service.ChargeCaptured(stripeEvent);
                    break;
                case "charge.updated":
                    await service.ChargeUpdated(stripeEvent);
                    break;
                case "customer.created":
                    await service.CustomerCreated(stripeEvent);
                    break;
                case "customer.updated":
                    await service.CustomerUpdated(stripeEvent);
                    break;
                case "customer.deleted":
                    await service.CustomerDeleted(stripeEvent);
                    break;
                case "customer.card.created":
                    await service.CustomerCardCreated(stripeEvent);
                    break;
                case "customer.card.updated":
                    await service.CustomerCardUpdated(stripeEvent);
                    break;
                case "customer.card.deleted":
                    await service.CustomerCardDeleted(stripeEvent);
                    break;
                case "customer.subscription.created":
                    await service.SubscriptionCreated(stripeEvent);
                    break;
                case "customer.subscription.updated":
                    await service.SubscriptionUpdated(stripeEvent);
                    break;
                case "customer.subscription.deleted":
                    await service.SubscriptionDeleted(stripeEvent);
                    break;
                case "customer.subscription.trial_will_end":
                    await service.SubscriptionTrialWillEnd(stripeEvent);
                    break;
                case "invoice.created":
                    await service.InvoiceCreated(stripeEvent);
                    break;
                case "invoice.payment_failed":
                    await service.InvoicePaymentFailed(stripeEvent);
                    break;
                case "invoice.payment_succeeded":
                    await service.InvoicePaymentSucceeded(stripeEvent);
                    break;
                case "invoice.updated":
                    await service.InvoiceUpdated(stripeEvent);
                    break;
                case "invoiceitem.created":
                    await service.InvoiceItemCreated(stripeEvent);
                    break;
                case "invoiceitem.updated":
                    await service.InvoiceItemUpdated(stripeEvent);
                    break;
                case "invoiceitem.deleted":
                    await service.InvoiceItemDeleted(stripeEvent);
                    break;
                case "plan.created":
                    await service.PlanCreated(stripeEvent);
                    break;
                case "plan.updated":
                    await service.PlanUpdated(stripeEvent);
                    break;
                case "plan.deleted":
                    await service.PlanDeleted(stripeEvent);
                    break;
                default:
                    await service.UnhandledWebHook(stripeEvent);
                    break;
            }
        }

    }
}