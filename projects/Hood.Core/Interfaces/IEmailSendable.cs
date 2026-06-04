using System.Collections.Generic;
using Hood.Models;
using SendGrid.Helpers.Mail;

namespace Hood.Interfaces
{
    public interface IEmailSendable
    {
        EmailAddress From { get; set; }
        EmailAddress ReplyTo { get; set; }

        EmailAddress To { get; }
        bool SendToRecipient { get; set; }

        List<EmailAddress> NotifyEmails { get; set; }
        string NotifyRole { get; set; }

        MailObject WriteToMailObject(MailObject message);
        MailObject WriteNotificationToMailObject(MailObject message);
    }
}
