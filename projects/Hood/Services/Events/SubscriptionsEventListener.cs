using Hood.Core;
using Hood.Events;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class SubscriptionsEventListener 
    {
        protected readonly IEventsService _eventService;

        public SubscriptionsEventListener(IEventsService events)
        {
            _eventService = events;
        }

        public void Configure()
        {
            _eventService.StripeWebhook += OnWebhookTriggered;
            _eventService.UserSubcriptionChanged += OnUserSubscriptionChanged;
        }

        public void OnUserSubscriptionChanged(object sender, UserSubscriptionChangeEventArgs e)
        {
            var env = Engine.Current.Resolve<IHostingEnvironment>();
            if (!env.IsProduction())
            {
                var logService = Engine.Current.Resolve<ILogService>();
                logService.AddLogAsync<SubscriptionsEventListener>($"User Subscription Changed Event: {e.Action}", new { EventData = e, Sender = sender.GetType().ToString() });
            }
        }

        public void OnWebhookTriggered(object sender, StripeWebHookTriggerArgs e)
        {
            var env = Engine.Current.Resolve<IHostingEnvironment>();
            if (!env.IsProduction())
            {
                var logService = Engine.Current.Resolve<ILogService>();
                logService.AddLogAsync<SubscriptionsEventListener>($"Webhook Triggered Event: {e.Action}", new { EventData = e, Sender = sender.GetType().ToString() });
            }
        }

    }

}
