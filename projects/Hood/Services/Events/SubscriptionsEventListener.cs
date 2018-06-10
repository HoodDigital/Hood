using Hood.Core;
using Hood.Events;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class SubscriptionsEventListener
    {
        protected readonly IEventsService _eventService;
        protected readonly IHostingEnvironment _env;

        public SubscriptionsEventListener(IEventsService events)
        {
            _env = EngineContext.Current.Resolve<IHostingEnvironment>();
            _eventService = events;
        }

        public void Configure()
        {
            _eventService.StripeWebhook += OnWebhookTriggered;
            _eventService.UserSubcriptionChanged += OnUserSubscriptionChanged;
        }

        public void OnUserSubscriptionChanged(object sender, UserSubscriptionChangeEventArgs e)
        {
            if (!_env.IsProduction())
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.AddLogAsync($"User Subscription Changed EVEnt: {e.Action}", JsonConvert.SerializeObject(new { EventData = e, Sender = sender.GetType().ToString() }), Models.LogType.Info, Models.LogSource.Subscriptions, null, e.Subscription?.Id.ToString(), nameof(UserSubscription), null);
            }
        }

        public void OnWebhookTriggered(object sender, StripeWebHookTriggerArgs e)
        {
            if (!_env.IsProduction())
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.AddLogAsync($"Webhook Triggered Event: {e.Action}", JsonConvert.SerializeObject(new { EventData = e, Sender = sender.GetType().ToString() }), Models.LogType.Info, Models.LogSource.Subscriptions, null, e.StripeEvent?.Id.ToString(), null, null);
            }
        }

    }

}
