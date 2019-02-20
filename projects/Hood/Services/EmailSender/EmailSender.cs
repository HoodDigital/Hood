using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Hood.Extensions;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Hood.Models;
using System.Linq;

namespace Hood.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISettingsRepository _settings;
        private Models.MailSettings _mail;
        private Models.BasicSettings _info;
        private readonly IRazorViewRenderer _renderer;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailSender(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository site,
            IRazorViewRenderer renderer)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _settings = site;
            _renderer = renderer;
        }

        private SendGridClient GetMailClient()
        {
            _info = _settings.GetBasicSettings();
            _mail = _settings.GetMailSettings();
            return new SendGridClient(_mail.SendGridKey);
        }

        public EmailAddress GetSiteFromEmail()
        {
            _info = _settings.GetBasicSettings();
            _mail = _settings.GetMailSettings();
            string siteTitle = _settings.GetSiteTitle();
            string fromName = _mail.FromName.IsSet() ? _mail.FromName : siteTitle.IsSet() ? siteTitle : "HoodCMS";
            string fromEmail = _mail.FromEmail.IsSet() ? _mail.FromEmail : _info.Email.IsSet() ? _info.Email : "info@hooddigital.com";
            return new EmailAddress(fromEmail, fromName);
        }

        public async Task<int> SendEmailAsync(string to, string subject, string body, EmailAddress from = null, string toName = null)
        {
            MailObject message = new MailObject
            {
                Html = body,
                Text = body,
                To = new EmailAddress(to, toName),
                ToName = toName,
                Subject = subject,
                PreHeader = subject
            };
            return await SendEmailAsync(message, Models.MailSettings.PlainTemplate, from);
        }

        public async Task<int> SendEmailAsync(MailObject message, string template = Models.MailSettings.PlainTemplate, EmailAddress from = null)
        {
            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();

            var html = await _renderer.Render(template, message);
            var textContent = new SendGrid.Helpers.Mail.Content("text/plain", message.ToString());
            var htmlContent = new SendGrid.Helpers.Mail.Content("text/html", html);

            var msg = MailHelper.CreateSingleEmail(from, message.To, message.Subject, message.ToString(), html);
            var response = await client.SendEmailAsync(msg);
            var body = await response.DeserializeResponseBodyAsync(response.Body);
            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> SendEmailAsync(EmailAddress[] emails, string subject, string htmlContent, string textContent = null, EmailAddress from = null)
        {
            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();
            int sent = 0;
            foreach (var email in emails)
            {
                var msg = MailHelper.CreateSingleEmail(from, email, subject, textContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
                var body = await response.DeserializeResponseBodyAsync(response.Body);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                    sent++;
            }
            return sent;
        }

        public async Task<int> NotifyRoleAsync(MailObject message, string roleName, string template = "Areas/Admin/Views/Mail/Plain.cshtml", EmailAddress from = null)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            int sent = 0;
            foreach (var user in users)
            {
                var messageToSend = message;
                messageToSend.To = new EmailAddress(user.Email);
                sent += await SendEmailAsync(messageToSend, template);
            }
            return sent;
        }
        public async Task<int> NotifyRoleAsync(string roleName, string subject, string htmlContent, string textContent = null, EmailAddress from = null)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            var emails = users.Select(u => new EmailAddress(u.Email, u.FullName)).ToArray();
            return await SendEmailAsync(emails, subject, htmlContent, textContent);
        }
    }
}