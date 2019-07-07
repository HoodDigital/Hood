using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IStripeWebHookService
    {
        Task ProcessEventAsync(string eventJson);

        Task CustomerCreatedAsync(Stripe.Event stripeEvent);
        Task CustomerDeletedAsync(Stripe.Event stripeEvent);
        Task CustomerUpdatedAsync(Stripe.Event stripeEvent);

        Task InvoicePaymentFailedAsync(Stripe.Event stripeEvent);
        Task InvoicePaymentSucceededAsync(Stripe.Event stripeEvent);

        Task PlanCreatedAsync(Stripe.Event stripeEvent);
        Task PlanUpdatedAsync(Stripe.Event stripeEvent);
        Task PlanDeletedAsync(Stripe.Event stripeEvent);

        Task SubscriptionCreatedAsync(Stripe.Event stripeEvent);
        Task SubscriptionUpdatedAsync(Stripe.Event stripeEvent);
        Task SubscriptionTrialWillEndAsync(Stripe.Event stripeEvent);
        Task SubscriptionDeletedAsync(Stripe.Event stripeEvent);
    }
}