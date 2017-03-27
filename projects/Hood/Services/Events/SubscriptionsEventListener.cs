using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    internal class SubscriptionsEventListener
    {
        private readonly EventsService _events;

        public SubscriptionsEventListener(EventsService events)
        {
            _events = events;
        }

        internal void Configure()
        {
            _events.StripeWebhook += OnWebhookTriggered;
            _events.UserSubcriptionChanged += OnUserSubscriptionChanged;
        }

        private void OnUserSubscriptionChanged(object sender, UserSubscriptionChangeEventArgs e)
        {
        }

        private void OnWebhookTriggered(object sender, StripeWebHookTriggerArgs e)
        {
        }

    }

}
