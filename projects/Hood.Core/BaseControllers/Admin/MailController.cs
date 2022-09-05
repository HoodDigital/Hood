using Hood.Core;
using Hood.BaseControllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using Hood.Models;

namespace Hood.Admin.BaseControllers
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
            mail.AddH1("Cras ac consectetur orci, eu.", align: "left");
            mail.AddH2("Aliquam tempor congue dui, quis.", align: "left");
            mail.AddH3("Nulla ac est blandit, pretium augue a, porta dui. ", align: "left");
            mail.AddH4("Nunc at pellentesque ligula, vel ullamcorper ipsum. Aenean lorem.", align: "left");
            mail.AddParagraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum at semper lorem, id varius leo. Vivamus tristique ac ante eu bibendum. Praesent quis dolor justo. Nullam interdum vestibulum tortor, vitae aliquet mauris faucibus id. Aenean sed massa quis dolor tempor mollis vitae sit amet erat. Donec eleifend, nisi consectetur gravida egestas, leo erat viverra mauris, a dapibus ligula nisi ut erat. Nulla laoreet scelerisque vulputate. Duis laoreet ex quis tellus elementum efficitur. Proin ut lorem a lorem blandit pulvinar vel a tellus. Cras in dolor bibendum, maximus eros eget, tristique erat. Aliquam commodo quis lacus quis volutpat. Suspendisse ut auctor mauris, eget venenatis sem. Suspendisse et leo vitae risus auctor malesuada et id lacus. Vestibulum a libero quis sapien varius pharetra lacinia elementum nulla. Nam tempus, turpis id faucibus congue, magna lorem aliquam orci, quis suscipit metus elit bibendum justo. Quisque sagittis consequat tellus, eget imperdiet risus congue non.", align: "left");
            mail.AddParagraph("Vivamus vehicula auctor maximus. Praesent purus dui, venenatis eu lectus et, convallis aliquam urna. Vestibulum pharetra imperdiet dolor, ac convallis risus sagittis eget. Sed nulla erat, tristique et erat id, viverra viverra nisi. Morbi ex libero, eleifend eget orci blandit, vestibulum bibendum nisl. In dapibus faucibus eros, sit amet iaculis turpis dignissim lacinia. Phasellus pretium odio vitae sem porttitor molestie. Morbi luctus est posuere lectus ullamcorper viverra. In sed facilisis purus.", align: "left");
            mail.AddParagraph("Sed semper lectus quis fermentum tincidunt. Maecenas nisi nisl, scelerisque non mi non, pharetra dapibus nibh. Sed ante orci, pretium eu ullamcorper a, laoreet at massa. Nam at arcu dui. Integer sollicitudin urna eget rutrum auctor. Nulla nec metus vel elit fermentum tincidunt. Donec placerat venenatis quam, at sodales mi cursus et. Etiam scelerisque, augue ac ullamcorper dictum, ex purus vestibulum quam, sit amet rutrum tortor arcu sed turpis. Curabitur ut turpis bibendum, semper neque non, maximus mi.", align: "left");
            mail.AddCallToAction("Ut non imperdiet", HttpContext.GetSiteUrl(), align: "left");
            mail.AddParagraph("And that's about it!", align: "left");
            return mail;
        }

    }
}
