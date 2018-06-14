using System;
using Stripe;
using Newtonsoft.Json;

namespace Hood.Events
{
    public class StripeWebHookTriggerArgs : EventArgs
    {
        public string Action { get; set;}
        public StripeEvent StripeEvent { get; set; }

        public StripeWebHookTriggerArgs(string json)
        {
            StripeEvent = StripeEventUtility.ParseEvent(json);
            Action = StripeEvent.Type;
        }
        public StripeWebHookTriggerArgs(StripeEvent stripeEvent)
        {
            StripeEvent = stripeEvent;
            Action = StripeEvent.Type;
        }
    }
}