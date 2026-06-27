using System.Net;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Hood.Services
{
    public class EmailSender : IEmailSender
    {
        protected Models.MailSettings _mail;
        protected BasicSettings _info;
        protected readonly IRazorViewRenderer _renderer;

        public EmailSender()
        {
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
            string fromName =
                _mail.FromName.IsSet() ? _mail.FromName
                : siteTitle.IsSet() ? siteTitle
                : "HoodCMS";
            string fromEmail =
                _mail.FromEmail.IsSet() ? _mail.FromEmail
                : _info.Email.IsSet() ? _info.Email
                : "info@hooddigital.com";
            return new EmailAddress(fromEmail, fromName);
        }

        public virtual async Task<int> SendEmailAsync(
            MailObject message,
            EmailAddress from = null,
            EmailAddress replyTo = null
        )
        {
            if (!Engine.Settings.Mail.SendGridKey.IsSet())
            {
                // Mail is not configured — log and silently skip so flows that send
                // email (register, contact forms) still complete on unconfigured sites.
                await Engine.Logs.AddLogAsync<EmailSender>(
                    "Email not sent: SendGrid is not set up. Add an API key in mail settings to enable email sending.",
                    message,
                    LogType.Warning
                );
                return 0;
            }

            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();

            var html = await _renderer.Render(message.Template, message);
            // Build a real text/plain alternative. Builder-API emails already hold proper plain text in
            // message.Text; template-rendered emails (empty Text) derive it from the rendered HTML. Never
            // pass message.ToString() — it returns the type name and trips the MPART_ALT_DIFF spam heuristic
            // (HOOD-139).
            var plainText = message.Text.IsSet() ? message.Text : html.HtmlToPlainText();
            var msg = MailHelper.CreateSingleEmail(
                from,
                message.To,
                message.Subject,
                plainText,
                html
            );
            msg.ReplyTo = replyTo;
            var response = await client.SendEmailAsync(msg);
            if (
                response.StatusCode == HttpStatusCode.Accepted
                || response.StatusCode == HttpStatusCode.OK
            )
                return 1;

            throw new System.Exception(
                "The email could not be sent, check your SendGrid settings."
            );
        }

        public virtual async Task<int> SendEmailAsync(
            EmailAddress[] emails,
            string subject,
            string htmlContent,
            string textContent = null,
            EmailAddress from = null,
            EmailAddress replyTo = null
        )
        {
            if (!Engine.Settings.Mail.SendGridKey.IsSet())
            {
                // Mail is not configured — log and silently skip so flows that send
                // email (register, contact forms) still complete on unconfigured sites.
                await Engine.Logs.AddLogAsync<EmailSender>(
                    "Email not sent: SendGrid is not set up. Add an API key in mail settings to enable email sending.",
                    subject,
                    LogType.Warning
                );
                return 0;
            }

            SendGridClient client = GetMailClient();
            if (from == null)
                from = GetSiteFromEmail();
            int sent = 0;
            foreach (var email in emails)
            {
                var msg = MailHelper.CreateSingleEmail(
                    from,
                    email,
                    subject,
                    textContent,
                    htmlContent
                );
                msg.ReplyTo = replyTo;
                var response = await client.SendEmailAsync(msg);
                if (
                    response.StatusCode == HttpStatusCode.Accepted
                    || response.StatusCode == HttpStatusCode.OK
                )
                    sent++;
            }
            return sent;
        }
    }
}
