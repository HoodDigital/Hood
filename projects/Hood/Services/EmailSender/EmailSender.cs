using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Hood.Extensions;
using System.Threading.Tasks;
using Hood.Infrastructure;
using System;
using System.Net;

namespace Hood.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISiteConfiguration _site;
        private Models.MailSettings _mail;
        private Models.BasicSettings _info;
        private readonly IRazorViewRenderer _renderer;

        public EmailSender(IHttpContextAccessor contextAccessor, ISiteConfiguration site, IRazorViewRenderer renderer)
        {
            _contextAccessor = contextAccessor;
            _site = site;
            _renderer = renderer;
        }

        public async Task<OperationResult> SendEmail(MailObject message, string template = Models.MailSettings.PlainTemplate)
        {
            try
            {
                _info = _site.GetBasicSettings();
                _mail = _site.GetMailSettings(true);

                string siteTitle = _site.GetSiteTitle();
                string fromName = _mail.FromName.IsSet() ? _mail.FromName : siteTitle.IsSet() ? siteTitle : "HoodCMS";
                string fromEmail = _mail.FromEmail.IsSet() ? _mail.FromEmail : _info.Email.IsSet() ? _info.Email : "info@hooddigital.com";

                var from = new Email(fromEmail, fromName);
                var html = await _renderer.Render(template, message);
                var textContent = new SendGrid.Helpers.Mail.Content("text/plain", message.ToString());
                var htmlContent = new SendGrid.Helpers.Mail.Content("text/html", html);

                var mail = new Mail(from, message.Subject, message.To, textContent);
                mail.AddContent(htmlContent);
                dynamic sg = new SendGridAPIClient(_mail.SendGridKey);
                dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
                HttpStatusCode status = response.StatusCode;
                string body = await response.Body.ReadAsStringAsync();
                if (status == HttpStatusCode.Accepted)
                {
                    return new OperationResult(true);
                }
                else
                {
                    return new OperationResult(body.ToFormattedJson());
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }
    }
}