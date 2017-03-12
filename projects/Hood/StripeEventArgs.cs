using System;
using Stripe;

namespace Hood
{
    public class StripeWebHookTriggerArgs : EventArgs
    {
        public StripeWebHookTriggerArgs(string json)
        {
            StripeEvent = StripeEventUtility.ParseEvent(json);
            Json = json;
        }
        public string Json { get; set; }
        public StripeEvent StripeEvent { get; set; }
    }
}