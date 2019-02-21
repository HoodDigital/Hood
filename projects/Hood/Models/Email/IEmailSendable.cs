using Hood.Services;
using SendGrid.Helpers.Mail;

namespace Hood.Models
{
    public interface IEmailSendable
    {
        EmailAddress To { get; }

        bool NotifySender { get; set; }
        EmailAddress NotifyEmail { get; set; }
        string NotifyRole { get; set; }

        MailObject WriteToMailObject(MailObject message);
        MailObject WriteNotificationToMailObject(MailObject message);
    }
}
