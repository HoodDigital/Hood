using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Core.Extensions
{
    public static class IStripeEventExtensions
    {
        public static string GetEventName(this StripeEvent stripeEvent)
        {
            return stripeEvent != null ? stripeEvent.Type != null ? stripeEvent.Type : "invalid.event.object" : "invalid.event.object";
        }
    }
}
