using System;
using Stripe;

namespace Hood.Events
{
    public class StripeWebHookTriggerArgs : EventArgs
    {
        public string Action { get; set;}
        public Stripe.Event Event { get; set; }

        public StripeWebHookTriggerArgs(string json)
        {
            Event = EventUtility.ParseEvent(json);
            Action = Event.Type;
        }
        public StripeWebHookTriggerArgs(Stripe.Event stripeEvent)
        {
            Event = stripeEvent;
            Action = Event.Type;
        }
    }
}