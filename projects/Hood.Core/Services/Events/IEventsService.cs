using System;
using Hood.Events;

namespace Hood.Services
{
    public interface IEventsService
    {
        event EventHandler<EventArgs> ContentChanged;
        event EventHandler<EventArgs> ForumChanged;
        event EventHandler<EventArgs> OptionsChanged;
        event EventHandler<EventArgs> PropertiesChanged;
        event EventHandler<StripeWebHookTriggerArgs> StripeWebhook;
        event EventHandler<UserSubscriptionChangeEventArgs> UserSubcriptionChanged;

        void TriggerContentChanged(object sender);
        void TriggerForumChanged(object sender);
        void TriggerOptionsChanged(object sender);
        void TriggerPropertiesChanged(object sender);
        void TriggerStripeWebhook(object sender, StripeWebHookTriggerArgs e);
        void TriggerUserSubcriptionChanged(object sender, UserSubscriptionChangeEventArgs e);
    }
}