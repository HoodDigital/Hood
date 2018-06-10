using System;
using Stripe;
using Newtonsoft.Json;

namespace Hood.Events
{
    public class StripeWebHookTriggerArgs : EventArgs
    {
        public string Action { get; set;}
        public string Json { get; set; }
        public StripeEvent StripeEvent { get; set; }

        public StripeWebHookTriggerArgs(string json)
        {
            StripeEvent = StripeEventUtility.ParseEvent(json);
            Json = json;
            Action = StripeEvent.Type;
        }
        public StripeWebHookTriggerArgs(StripeEvent stripeEvent)
        {
            StripeEvent = stripeEvent;
            Json = JsonConvert.SerializeObject(stripeEvent);
            Action = StripeEvent.Type;
        }
    }
}