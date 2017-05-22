using Stripe;

namespace Hood.Extensions
{
    public static class StripeEventExtensions
    {
        public static string GetEventName(this StripeEvent stripeEvent)
        {
            return stripeEvent != null ? stripeEvent.Type != null ? stripeEvent.Type : "invalid.event.object" : "invalid.event.object";
        }
    }
}
