using System;
using Hood.Services;

namespace Hood.Models
{
    public class WelcomeEmailModel : IEmailSendable
    {
        public WelcomeEmailModel(HoodIdentityUser user)
        {
            User = user;
        }

        public HoodIdentityUser User { get; set; }
        public MailObject WriteToMessage(MailObject message)
        {
            message.Subject = User.ReplacePlaceholders(message.Subject);
            message.Text = User.ReplacePlaceholders(message.Text);
            message.Html = User.ReplacePlaceholders(message.Html);
            message.PreHeader = User.ReplacePlaceholders(message.PreHeader);
            message.AddParagraph("Your username: <strong>" + User.UserName + "</strong>");
            return message;
        }
    }
}