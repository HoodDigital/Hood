using System;

namespace Hood.Enums
{
    [Obsolete("Use inline messages now, with TempData and SaveMessage/MessageType.",true)]
    public enum EditorMessage
    {
        Saved,
        Deleted,
        Created,
        ImageUpdated,
        MediaRemoved,
        Error,
        HomepageSet,
        Archived,
        Published,
        Succeeded,
        Sent,
        ErrorSending,
        NotFound,
        ErrorDuplicating,
        Duplicated,
        CannotDeleteAdmin,
        Deactivated,
        Activated,
        KeyRolled,
        KeyCreated,
        Exists
    }
}
