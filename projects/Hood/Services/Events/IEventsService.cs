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

        void triggerContentChanged(object sender);
        void triggerForumChanged(object sender);
        void triggerOptionsChanged(object sender);
        void triggerPropertiesChanged(object sender);
        void triggerStripeWebhook(object sender, StripeWebHookTriggerArgs e);
        void triggerUserSubcriptionChanged(object sender, UserSubscriptionChangeEventArgs e);
    }
}