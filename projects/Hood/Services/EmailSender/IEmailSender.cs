using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IEmailSender
    {
        EmailAddress GetSiteFromEmail();
        Task<int> SendEmailAsync(string to, string subject, string body, EmailAddress from = null, string toName = null);
        Task<int> SendEmailAsync(MailObject message, string template = Models.MailSettings.PlainTemplate, EmailAddress from = null);
        Task<int> SendEmailAsync(EmailAddress[] emails, string subject, string htmlContent, string textContent = null, EmailAddress from = null);
        Task<int> NotifyRoleAsync(MailObject message, string roleName, string template = Models.MailSettings.PlainTemplate, EmailAddress from = null);
        Task<int> NotifyRoleAsync(string roleName, string subject, string htmlContent, string textContent = null, EmailAddress from = null);
    }
}
