using Hood.Core;
using Hood.Events;
using System;
using System.Linq;

namespace Hood.Services
{
    public class EventsService : IEventsService
    {
        private event EventHandler<EventArgs> _ForumChanged;
        public event EventHandler<EventArgs> ForumChanged
        {
            add
            {
                if (_ForumChanged == null || !_ForumChanged.GetInvocationList().Contains(value))
                {
                    _ForumChanged += value;
                }
            }
            remove
            {
                _ForumChanged -= value;
            }
        }
        public void triggerForumChanged(object sender)
        {
            try
            {
                foreach (var d in _ForumChanged.GetInvocationList())
                {
                    d.DynamicInvoke(sender);
                }
            }
            catch (Exception ex)
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.LogErrorAsync("An error while triggering a Forum Changed event handler.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IEventsService), null);
            }
        }

        private event EventHandler<EventArgs> _ContentChanged;
        public event EventHandler<EventArgs> ContentChanged
        {
            add
            {
                if (_ContentChanged == null || !_ContentChanged.GetInvocationList().Contains(value))
                {
                    _ContentChanged += value;
                }
            }
            remove
            {
                _ContentChanged -= value;
            }
        }
        public void triggerContentChanged(object sender)
        {
            _ContentChanged?.Invoke(sender, new EventArgs());
        }

        private event EventHandler<EventArgs> _PropertiesChanged;
        public event EventHandler<EventArgs> PropertiesChanged
        {
            add
            {
                if (_PropertiesChanged == null || !_PropertiesChanged.GetInvocationList().Contains(value))
                {
                    _PropertiesChanged += value;
                }
            }
            remove
            {
                _PropertiesChanged -= value;
            }
        }
        public void triggerPropertiesChanged(object sender)
        {
            try
            {
                foreach (var d in _PropertiesChanged.GetInvocationList())
                {
                    d.DynamicInvoke(sender);
                }
            }
            catch (Exception ex)
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.LogErrorAsync("An error while triggering a Properties Changed event handler.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IEventsService), null);
            }
        }

        private event EventHandler<EventArgs> _OptionsChanged;
        public event EventHandler<EventArgs> OptionsChanged
        {
            add
            {
                if (_OptionsChanged == null || !_OptionsChanged.GetInvocationList().Contains(value))
                {
                    _OptionsChanged += value;
                }
            }
            remove
            {
                _OptionsChanged -= value;
            }
        }
        public void triggerOptionsChanged(object sender)
        {
            try
            {
                foreach (var d in _OptionsChanged.GetInvocationList())
                {
                    d.DynamicInvoke(sender);
                }
            }
            catch (Exception ex)
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.LogErrorAsync("An error while triggering a Options Changed event handler.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IEventsService), null);
            }
        }

        private event EventHandler<UserSubscriptionChangeEventArgs> _UserSubcriptionChanged;
        public event EventHandler<UserSubscriptionChangeEventArgs> UserSubcriptionChanged
        {
            add
            {
                if (_UserSubcriptionChanged == null || !_UserSubcriptionChanged.GetInvocationList().Contains(value))
                {
                    _UserSubcriptionChanged += value;
                }
            }
            remove
            {
                _UserSubcriptionChanged -= value;
            }
        }
        public void triggerUserSubcriptionChanged(object sender, UserSubscriptionChangeEventArgs e)
        {
            try
            {
                foreach (var d in _UserSubcriptionChanged.GetInvocationList())
                {
                    d.DynamicInvoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.LogErrorAsync("An error while triggering a User Subcription Changed event handler.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IEventsService), null);
            }
        }

        private event EventHandler<StripeWebHookTriggerArgs> _StripeWebhook;
        public event EventHandler<StripeWebHookTriggerArgs> StripeWebhook
        {
            add
            {
                if (_StripeWebhook == null || !_StripeWebhook.GetInvocationList().Contains(value))
                {
                    _StripeWebhook += value;
                }
            }
            remove
            {
                _StripeWebhook -= value;
            }
        }
        public void triggerStripeWebhook(object sender, StripeWebHookTriggerArgs e)
        {
            try
            {
                foreach (var d in _StripeWebhook.GetInvocationList())
                {
                    d.DynamicInvoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                var logService = EngineContext.Current.Resolve<ILogService>();
                logService.LogErrorAsync("An error while triggering a Stripe Webhook event handler.", ex, Models.LogType.Error, Models.LogSource.System, null, null, nameof(IEventsService), null);
            }
        }
    }
}
