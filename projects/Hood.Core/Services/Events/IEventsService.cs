using System;

namespace Hood.Services
{
    public interface IEventsService
    {
        event EventHandler<EventArgs> ContentChanged;
        event EventHandler<EventArgs> ForumChanged;
        event EventHandler<EventArgs> OptionsChanged;
        event EventHandler<EventArgs> PropertiesChanged;

        void TriggerContentChanged(object sender);
        void TriggerForumChanged(object sender);
        void TriggerOptionsChanged(object sender);
        void TriggerPropertiesChanged(object sender);
    }
}