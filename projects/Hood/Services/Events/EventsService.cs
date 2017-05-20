using Hood.Events;
using System;
using System.Linq;

namespace Hood.Services
{
    public class EventsService
    {
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
            _PropertiesChanged?.Invoke(sender, new EventArgs());
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
            _OptionsChanged?.Invoke(sender, new EventArgs());
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
            _UserSubcriptionChanged?.Invoke(sender, e);
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
            _StripeWebhook?.Invoke(sender, e);
        }
    }
}
