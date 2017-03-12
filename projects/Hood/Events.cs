using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood
{
    public static class Events
    {
        public static void Fire(string eventName, object sender = null, EventArgs e = null)
        {
            switch (eventName)
            {
                case nameof(ContentChanged):
                    ContentChanged?.Invoke(sender, e);
                    break;
                case nameof(PropertiesChanged):
                    PropertiesChanged?.Invoke(sender, e);
                    break;
                case nameof(OptionsChanged):
                    OptionsChanged?.Invoke(sender, e);
                    break;
            }
        }

        public static event EventHandler<EventArgs> ContentChanged;
        public static event EventHandler<EventArgs> PropertiesChanged;
        public static event EventHandler<EventArgs> OptionsChanged;
        public static event EventHandler<StripeWebHookTriggerArgs> StripeWebhook;
    }
}
