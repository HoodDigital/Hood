using Hood.Events;

namespace Hood.Services
{
    public class SubscriptionsEventListener
    {
        private readonly EventsService _events;

        public SubscriptionsEventListener(EventsService events)
        {
            _events = events;
        }

        public void Configure()
        {
            _events.StripeWebhook += OnWebhookTriggered;
            _events.UserSubcriptionChanged += OnUserSubscriptionChanged;
        }

        public void OnUserSubscriptionChanged(object sender, UserSubscriptionChangeEventArgs e)
        {
        }

        public void OnWebhookTriggered(object sender, StripeWebHookTriggerArgs e)
        {
        }

    }

}
