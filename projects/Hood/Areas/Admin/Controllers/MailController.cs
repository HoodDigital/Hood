using System;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;
using Hood.Enums;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="SuperUser,Admin")]
    public class MailController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly IContentRepository _content;
        private readonly ISettingsRepository _settings;
        private readonly IAccountRepository _auth;
        private readonly IEmailSender _email;

        public MailController(IAccountRepository auth,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISettingsRepository site,
                              IContentRepository content,
                              IEmailSender email)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _settings = site;
            _email = email;
        }
        [Route("admin/mail/preview/plain/")]
        public IActionResult Plain()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/warning/")]
        public IActionResult Warning()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/danger/")]
        public IActionResult Danger()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/success/")]
        public IActionResult Success()
        {
            return View(GetDemoMail());
        }

        [HttpPost]
        [Route("admin/mail/test/")]
        public async Task<IActionResult> Test(string email = "info@hooddigital.com", string template = Models.MailSettings.PlainTemplate)
        {
            try
            {
                MailObject mail = GetDemoMail();
                mail.To = new SendGrid.Helpers.Mail.EmailAddress(email);
                mail.Subject = "Test email from HoodCMS";
                mail.PreHeader = "This is a test email from HoodCMS.";
                await _email.SendEmailAsync(mail, template);
                return RedirectToAction("Mail", "Settings", new { status = EditorMessage.Sent });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Mail", "Settings", new { status = EditorMessage.ErrorSending });
            }
        }

        [Route("hood/test-email-full/")]
        public IActionResult Terms()
        {
            var mailSettings = _settings.GetMailSettings();
            var mail = GetDemoMail();

            mail.Subject = "Testing basic email sends";
            mail.To = _email.GetSiteFromEmail();
            _email.SendEmailAsync(mail);
            _email.NotifyRoleAsync(mail, "Admin");
            EmailAddress[] emails = { mail.To };
            _email.SendEmailAsync(emails, "Basic Email send", "<h1>basic email test</h1>", "basic email test");
            _email.NotifyRoleAsync("Admin", "Basic Email send", "<h1>basic email test</h1>", "basic email test");

            mail.Subject = "Testing FROM parameter";

            var from = new EmailAddress() { Email = "george@hooddigital.com", Name = "HoodCMS Test From" };

            _email.SendEmailAsync(mail, Models.MailSettings.DangerTemplate, from);
            _email.NotifyRoleAsync(mail, "Admin", Models.MailSettings.DangerTemplate, from);
            _email.SendEmailAsync(emails, "Testing FROM parameter - BASIC", "<h1>basic email test</h1>", "basic email test", from);
            _email.NotifyRoleAsync("Admin", "Testing FROM parameter - BASIC", "<h1>basic email test</h1>", "basic email test", from);

            return View();
        }

        protected MailObject GetDemoMail()
        {
            MailObject mail = new MailObject()
            {
                Subject = "Plain old email",
                PreHeader = "Some preheader text..."
            };
            mail.AddH1("This is the heading 1!", align: "left");
            mail.AddH2("This is the heading 2!", align: "left");
            mail.AddH3("This is the heading 3!", align: "left");
            mail.AddH4("This is the heading 4!", align: "left");
            mail.AddParagraph("Here is the first old paragraph in the old email we are sending... this is added by code!", align: "left");
            mail.AddCallToAction("Some groovy button", HttpContext.GetSiteUrl(), align: "left");
            mail.AddParagraph("And that's about it!", align: "left");
            return mail;
        }

    }
}
