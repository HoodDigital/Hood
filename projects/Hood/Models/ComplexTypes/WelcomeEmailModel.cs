using Hood.Services;
using Hood.Extensions;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class WelcomeEmailModel : IEmailSendable
    {
        public WelcomeEmailModel(ApplicationUser user, string confirmationLink = null)
        {
            User = user;
            ConfirmationLink = confirmationLink;
        }

        [JsonConverter(typeof(ApplicationUserJsonConverter))]
        public ApplicationUser User { get; set; }
        public string ConfirmationLink { get; set; }

        public MailObject WriteToMessage(MailObject message)
        {
            message.Subject = User.ReplacePlaceholders(message.Subject);
            message.Text = User.ReplacePlaceholders(message.Text);
            message.Html = User.ReplacePlaceholders(message.Html);
            message.PreHeader = User.ReplacePlaceholders(message.PreHeader);
            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");
            if (ConfirmationLink.IsSet())
            {
                message.AddParagraph("Please confirm your account by clicking the link below, we just need to make sure you are a real person.");
                message.AddCallToAction("Confirm your account", ConfirmationLink);
            }
            return message;
        }
    }
}