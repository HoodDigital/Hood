using Hood.Models;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IEmailSender
    {
        EmailAddress GetSiteFromEmail();
        Task<int> SendEmailAsync(MailObject message, EmailAddress from = null, EmailAddress replyTo = null);
        Task<int> SendEmailAsync(EmailAddress[] emails, string subject, string htmlContent, string textContent = null, EmailAddress from = null, EmailAddress replyTo = null);
        Task<int> NotifyRoleAsync(MailObject message, string roleName, EmailAddress from = null, EmailAddress replyTo = null);
        Task<int> NotifyRoleAsync(string roleName, string subject, string htmlContent, string textContent = null, EmailAddress from = null, EmailAddress replyTo = null);
    }
}
