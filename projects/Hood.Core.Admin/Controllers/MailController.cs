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
using Hood.Models;

namespace Hood.Areas.Admin.Controllers
{
    public abstract class BaseMailController : BaseController
    {
        public BaseMailController()
            : base()
        {
        }

        [Route("admin/mail/preview/plain/")]
        public virtual IActionResult Plain()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/warning/")]
        public virtual IActionResult Warning()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/danger/")]
        public virtual IActionResult Danger()
        {
            return View(GetDemoMail());
        }

        [Route("admin/mail/preview/success/")]
        public virtual IActionResult Success()
        {
            return View(GetDemoMail());
        }

        [HttpPost]
        [Route("admin/mail/test/")]
        public virtual async Task<IActionResult> Test(string email = "info@hooddigital.com", string template = Models.MailSettings.PlainTemplate)
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
                await _logService.AddExceptionAsync<BaseMailController>(SaveMessage, ex);
            }
            return RedirectToAction("Mail", "Settings");
        }

        protected virtual MailObject GetDemoMail()
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
