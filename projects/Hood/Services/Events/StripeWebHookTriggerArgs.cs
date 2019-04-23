using System;
using Stripe;
using Newtonsoft.Json;

namespace Hood.Events
{
    public class StripeWebHookTriggerArgs : EventArgs
    {
        public string Action { get; set;}
        public Stripe.Event Event { get; set; }

        public StripeWebHookTriggerArgs(string json)
        {
            Event = Stripe.EventUtility.ParseEvent(json);
            Action = Event.Type;
        }
        public StripeWebHookTriggerArgs(Stripe.Event stripeEvent)
        {
            Event = stripeEvent;
            Action = Event.Type;
        }
    }
}