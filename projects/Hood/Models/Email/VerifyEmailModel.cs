using Hood.Services;
using Hood.Extensions;
using Hood.Core;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using Hood.Interfaces;

namespace Hood.Models
{
    public class VerifyEmailModel : IEmailSendable
    {
        public VerifyEmailModel(ApplicationUser user, string confirmationLink = null)
        {
            User = user;
            ConfirmLink = confirmationLink;
        }

        public EmailAddress From { get; set; } = null;

        public ApplicationUser User { get; set; }
        public string ConfirmLink { get; }

        public EmailAddress To
        {
            get
            {
                return new EmailAddress(User.Email, User.ToFullName());
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
            message.Subject = _accountSettings.VerifySubject.IsSet() ? _accountSettings.VerifySubject.ReplaceSiteVariables().ReplaceUserVariables(User) : "Verify your email";
            message.PreHeader = _accountSettings.VerifyTitle.IsSet() ? _accountSettings.VerifyTitle.ReplaceSiteVariables().ReplaceUserVariables(User) : "You have been sent this in order to validate your email.";;

            message.AddH1(_accountSettings.VerifyTitle.ReplaceSiteVariables().ReplaceUserVariables(User));
            message.AddDiv(_accountSettings.VerifyMessage.ReplaceSiteVariables().ReplaceUserVariables(User));
            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");

            if (ConfirmLink.IsSet())
            {
                message.AddParagraph("Please click the link below to confirm your email.");
                message.AddCallToAction("Confirm your Email", ConfirmLink);
                message.AddParagraph($"Or visit the following URL: {ConfirmLink}");
            }
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