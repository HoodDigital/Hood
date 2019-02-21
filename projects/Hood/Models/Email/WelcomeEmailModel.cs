using Hood.Services;
using Hood.Extensions;
using Newtonsoft.Json;
using Hood.Core;
using SendGrid.Helpers.Mail;

namespace Hood.Models
{
    public class WelcomeEmailModel : IEmailSendable
    {
        public WelcomeEmailModel(ApplicationUser user, string loginLink = null)
        {
            User = user;
            LoginLink = loginLink;
        }

        [JsonConverter(typeof(ApplicationUserJsonConverter))]
        public ApplicationUser User { get; set; }
        public string LoginLink { get; }

        public EmailAddress To
        {
            get
            {
                return new EmailAddress(User.Email, User.FullName);
            }
        }

        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public bool NotifySender { get; set; }
        public EmailAddress NotifyEmail { get; set; }
        public string NotifyRole { get; set; }

        public MailObject WriteToMailObject(MailObject message)
        {
            var _settings = Engine.Current.Resolve<ISettingsRepository>();
            var _accountSettings = _settings.GetAccountSettings();
            message.Subject = User.ReplacePlaceholders(message.Subject);
            message.PreHeader = User.ReplacePlaceholders(message.PreHeader);

            message.AddH1(_settings.ReplacePlaceholders(_accountSettings.WelcomeTitle));
            message.AddDiv(_accountSettings.WelcomeMessage);
            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");

            if (LoginLink.IsSet())
            {
                message.AddParagraph("You can log in and access your account by clicking the link below.");
                message.AddCallToAction("Access your account", LoginLink);
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