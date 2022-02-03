using Hood.Extensions;
using Hood.Core;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using Hood.Interfaces;

namespace Hood.Models
{
    public class VerifyEmailModel : IEmailSendable
    {
        public VerifyEmailModel(ApplicationUser user, string confirmationLink)
        {
            User = user;
            ConfirmLink = confirmationLink;
        }

        public EmailAddress From { get; set; } = null;
        public EmailAddress ReplyTo { get; set; } = null;

        public ApplicationUser User { get; set; }
        public string ConfirmLink { get; }

        public EmailAddress To
        {
            get
            {
                return new EmailAddress(User.Email, User.ToInternalName());
            }
        }

        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public bool SendToRecipient { get; set; }
        public List<EmailAddress> NotifyEmails { get; set; }
        public string NotifyRole { get; set; }

        public string Template { get; set; } = MailSettings.SuccessTemplate;

        public MailObject WriteToMailObject(MailObject message)
        {
            var _accountSettings = Engine.Settings.Account;

            if (_accountSettings.VerifySubject.IsSet())
                message.Subject = _accountSettings.VerifySubject.ReplaceUserVariables(User).ReplaceSiteVariables();
            else
                message.Subject = "Confirm your email address for {Site.Title}.".ReplaceUserVariables(User).ReplaceSiteVariables();

            if (_accountSettings.VerifyTitle.IsSet())
                message.PreHeader = _accountSettings.VerifyTitle.ReplaceUserVariables(User).ReplaceSiteVariables();
            else
                message.PreHeader = "Confirm your email address.";

            if (_accountSettings.VerifyMessage.IsSet())
                message.AddDiv(_accountSettings.VerifyMessage.ReplaceSiteVariables().ReplaceUserVariables(User));
            else
                message.AddParagraph("You have been sent this in order to confirm your email.");

            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");

            message.AddParagraph("Please click the link below to confirm your email.");
            message.AddCallToAction("Confirm your Email", ConfirmLink);
            message.AddParagraph($"Or visit the following URL: {ConfirmLink}");

            return message;
        }

        public MailObject WriteNotificationToMailObject(MailObject message)
        {
            message = WriteToMailObject(message);
            message.Subject += " [COPY]";
            return message;
        }
    }
}