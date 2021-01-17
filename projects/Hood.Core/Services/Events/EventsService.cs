using Hood.Core;
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
        public void TriggerForumChanged(object sender)
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
                var logService = Engine.Services.Resolve<ILogService>();
                logService.AddExceptionAsync<EventsService>("An error while triggering an event handler: " + nameof(TriggerForumChanged), ex);
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
        public void TriggerContentChanged(object sender)
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
        public void TriggerPropertiesChanged(object sender)
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
                var logService = Engine.Services.Resolve<ILogService>();
                logService.AddExceptionAsync<EventsService>("An error while triggering an event handler: " + nameof(TriggerPropertiesChanged), ex);
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
        public void TriggerOptionsChanged(object sender)
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
                var logService = Engine.Services.Resolve<ILogService>();
                logService.AddExceptionAsync<EventsService>("An error while triggering an event handler: " + nameof(TriggerOptionsChanged), ex);
            }
        }
    }
}
