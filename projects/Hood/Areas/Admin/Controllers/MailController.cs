using Hood.Core;
using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="SuperUser,Admin")]
    public class MailController : BaseController
    {
        public MailController()
            : base()
        {
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
                mail.Template = template;
                await _emailSender.SendEmailAsync(mail);

                SaveMessage = $"Test message sent to {email}.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error sending test email to {email}.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<MailController>(SaveMessage, ex);
            }
            return RedirectToAction("Mail", "Settings");
        }

        [Route("hood/test-email-full/")]
        public IActionResult Terms()
        {
            var mailSettings = Engine.Settings.Mail;
            var mail = GetDemoMail();

            mail.Subject = "Testing basic email sends";
            mail.To = _emailSender.GetSiteFromEmail();
            _emailSender.SendEmailAsync(mail);
            _emailSender.NotifyRoleAsync(mail, "Admin");
            EmailAddress[] emails = { mail.To };
            _emailSender.SendEmailAsync(emails, "Basic Email send", "<h1>basic email test</h1>", "basic email test");
            _emailSender.NotifyRoleAsync("Admin", "Basic Email send", "<h1>basic email test</h1>", "basic email test");

            mail.Subject = "Testing FROM parameter";

            var from = new EmailAddress() { Email = "george@hooddigital.com", Name = "HoodCMS Test From" };
            mail.Template = Models.MailSettings.DangerTemplate;

            _emailSender.SendEmailAsync(mail, from);
            _emailSender.NotifyRoleAsync(mail, "Admin", from);
            _emailSender.SendEmailAsync(emails, "Testing FROM parameter - BASIC", "<h1>basic email test</h1>", "basic email test", from);
            _emailSender.NotifyRoleAsync("Admin", "Testing FROM parameter - BASIC", "<h1>basic email test</h1>", "basic email test", from);

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
