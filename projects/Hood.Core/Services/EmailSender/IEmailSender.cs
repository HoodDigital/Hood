using System.Threading.Tasks;
using Hood.Models;
using SendGrid.Helpers.Mail;

namespace Hood.Services
{
    public interface IEmailSender
    {
        EmailAddress GetSiteFromEmail();
        Task<int> SendEmailAsync(
            MailObject message,
            EmailAddress from = null,
            EmailAddress replyTo = null
        );
        Task<int> SendEmailAsync(
            EmailAddress[] emails,
            string subject,
            string htmlContent,
            string textContent = null,
            EmailAddress from = null,
            EmailAddress replyTo = null
        );
    }
}
