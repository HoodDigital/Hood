using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Hood.Extensions;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Hood.Models;
using System.Linq;
using Hood.Interfaces;
using Hood.Core;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class EmailSender : IEmailSender
    {
        protected readonly IHttpContextAccessor _contextAccessor;
        protected Models.MailSettings _mail;
        protected Models.BasicSettings _info;
        protected readonly IRazorViewRenderer _renderer;        
        private UserManager<ApplicationUser> UserManager => Engine.Services.Resolve<UserManager<ApplicationUser>>();
 

        public EmailSender()
        {
            _contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
            _renderer = Engine.Services.Resolve<IRazorViewRenderer>();
        }

        protected SendGridClient GetMailClient()
        {
            _info = Engine.Settings.Basic;
            _mail = Engine.Settings.Mail;
            return new SendGridClient(_mail.SendGridKey);
        }

        public virtual EmailAddress GetSiteFromEmail()
        {
            _info = Engine.Settings.Basic;
            _mail = Engine.Settings.Mail;
            string siteTitle = Engine.Settings.Basic.FullTitle;
            string fromName = _mail.FromName.IsSet() ? _mail.FromName : siteTitle.IsSet() ? siteTitle : "HoodCMS";
            string fromEmail = _mail.FromEmail.IsSet() ? _mail.FromEmail : _info.Email.IsSet() ? _info.Email : "info@hooddigital.com";
            return new EmailAddress(fromEmail, fromName);
        }

        public virtual async Task<int> SendEmailAsync(MailObject message, EmailAddress from = null, EmailAddress replyTo = null)
        {
            if (!Engine.Settings.Mail.SendGridKey.IsSet())
            {
                throw new System.Exception("SendGrid is not setup. Create a free account and set up an API key to send emails.");
            }

            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();

            var html = await _renderer.Render(message.Template, message);
            var msg = MailHelper.CreateSingleEmail(from, message.To, message.Subject, message.ToString(), html);
            msg.ReplyTo = replyTo;
            var response = await client.SendEmailAsync(msg);
            var body = await response.DeserializeResponseBodyAsync(response.Body);
            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                return 1;

            throw new System.Exception("The email could not be sent, check your SendGrid settings.");
        }
        public virtual async Task<int> SendEmailAsync(EmailAddress[] emails, string subject, string htmlContent, string textContent = null, EmailAddress from = null, EmailAddress replyTo = null)
        {
            if (!Engine.Settings.Mail.SendGridKey.IsSet())
            {
                throw new System.Exception("SendGrid is not setup. Create a free account and set up an API key to send emails.");
            }

            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();
            int sent = 0;
            foreach (var email in emails)
            {
                var msg = MailHelper.CreateSingleEmail(from, email, subject, textContent, htmlContent);
                msg.ReplyTo = replyTo;
                var response = await client.SendEmailAsync(msg);
                var body = await response.DeserializeResponseBodyAsync(response.Body);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                    sent++;
            }
            return sent;
        }

        public virtual async Task<int> NotifyRoleAsync(MailObject message, string roleName, EmailAddress from = null, EmailAddress replyTo = null)
        {
            var users = await UserManager.GetUsersInRoleAsync(roleName);
            int sent = 0;
            foreach (var user in users)
            {
                var messageToSend = message;
                messageToSend.To = new EmailAddress(user.Email);
                sent += await SendEmailAsync(messageToSend, from, replyTo);
            }
            return sent;
        }
        public virtual async Task<int> NotifyRoleAsync(string roleName, string subject, string htmlContent, string textContent = null, EmailAddress from = null, EmailAddress replyTo = null)
        {
            var users = await UserManager.GetUsersInRoleAsync(roleName);
            var emails = users.Select(u => new EmailAddress(u.Email, u.ToFullName())).ToArray();
            return await SendEmailAsync(emails, subject, htmlContent, textContent, from, replyTo);
        }
    }
}