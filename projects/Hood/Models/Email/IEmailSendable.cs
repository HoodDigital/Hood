using Hood.Services;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace Hood.Models
{
    public interface IEmailSendable
    {
        EmailAddress From { get; set; }

        EmailAddress To { get; }
        bool SendToRecipient { get; set; }

        List<EmailAddress> NotifyEmails { get; set; }
        string NotifyRole { get; set; }

        MailObject WriteToMailObject(MailObject message);
        MailObject WriteNotificationToMailObject(MailObject message);
    }
}
