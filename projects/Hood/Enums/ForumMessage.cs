using System;

namespace Hood.Enums
{
    [Obsolete("Use inline messages now, with TempData and SaveMessage/MessageType.", true)]
    public enum ForumMessage
    {
        Saved,
        Deleted,
        Created,
        ImageUpdated,
        MediaRemoved,
        Error,
        Archived,
        Published,
        Succeeded,
        Sent,
        ErrorSending,
        NotFound,
        NoPostFound,
        PostReported,
        PostDeleted,
        TopicDeleted,
        NoTopicFound
    }
}