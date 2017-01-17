namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with all stripe functionality.
    /// </summary>
    public interface IBillingService
    {
        IStripeService Stripe { get; }
        ISubscriptionPlanService SubscriptionPlans { get; }
        ISubscriptionService Subscriptions { get; }
        ICustomerService Customers { get; }
        ICardService Cards { get; }
        IInvoiceService Invoices { get; }
    }
}
