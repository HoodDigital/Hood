﻿using Hood.Services;
using Hood.Extensions;
using Newtonsoft.Json;
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

        [JsonConverter(typeof(ApplicationUserJsonConverter))]
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

        public string Template { get; set; } = Hood.Models.MailSettings.SuccessTemplate;

        public MailObject WriteToMailObject(MailObject message)
        {
            var _settings = Engine.Current.Resolve<ISettingsRepository>();
            var _accountSettings = _settings.GetAccountSettings();
            message.Subject = User.ReplacePlaceholders(message.Subject);
            message.PreHeader = User.ReplacePlaceholders(message.PreHeader);

            message.AddH1(_settings.ReplacePlaceholders(_accountSettings.WelcomeTitle));
            message.AddDiv(_accountSettings.WelcomeMessage);
            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");

            if (ConfirmLink.IsSet())
            {
                message.AddParagraph("You can log in and access your account by clicking the link below.");
                message.AddCallToAction("Access your account", ConfirmLink);
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