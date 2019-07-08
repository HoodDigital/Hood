namespace Hood.Services
{
    /// <summary>
    /// Interface to provide direct communication with the Stripe Api
    /// </summary>
    public interface IStripeService
    {
        Stripe.PlanService PlanService { get; }
        Stripe.SubscriptionService SubscriptionService { get; }
        Stripe.CustomerService CustomerService { get; }
        Stripe.CardService CardService { get; }
        Stripe.ChargeService ChargeService { get; }
        Stripe.InvoiceService InvoiceService { get; }
        Stripe.InvoiceItemService InvoiceItemService { get; }
        Stripe.TokenService TokenService { get; }
        Stripe.RefundService RefundService { get; }
    }
}
