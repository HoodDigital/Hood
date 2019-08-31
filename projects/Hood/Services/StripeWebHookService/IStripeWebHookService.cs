using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IStripeWebHookService
    {
        Task ProcessEventAsync(Stripe.Event stripeEvent);
    }
}