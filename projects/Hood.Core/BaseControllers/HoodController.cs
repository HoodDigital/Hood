﻿using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unsplasharp;

namespace Hood.BaseControllers
{
    public abstract class HoodController : BaseController
    {
        public HoodController()
            : base()
        { }

        [HttpPost]
        [Route("hood/contact/send/")]
        [Route("hood/process-contact-form/")]
        [ValidateAntiForgeryToken]
        public virtual async Task<Response> ProcessContactForm(ContactFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response("The submitted information is not valid.");
                }

                var recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Passed)
                    return new Response("You have failed to pass the reCaptcha check. Please refresh your page and try again.");

                model.SendToRecipient = true;
                model.NotifyRole = "ContactFormNotifications";
                model.NotifyEmails = new System.Collections.Generic.List<SendGrid.Helpers.Mail.EmailAddress>()
                {
                    new SendGrid.Helpers.Mail.EmailAddress(Engine.Settings.Contact.Email, Engine.Settings.Basic.FullTitle)
                };

                return await _mailService.ProcessAndSend(model);
            }
            catch (Exception ex)
            {
                return new Models.Response(ex);
            }
        }

        [Route("robots.txt")]
        public virtual IActionResult Robots()
        {
            var sw = new StringWriter();
            //write the header
            sw.WriteLine("User-agent: *");
            sw.WriteLine("Disallow: /admin/ ");
            sw.WriteLine("Disallow: /account/ ");
            sw.WriteLine("Disallow: /install/ ");
            foreach (ContentType ct in Engine.Settings.Content.RestrictedTypes)
            {
                sw.WriteLine("Disallow: /" + ct.Slug + "/ ");
            }
            sw.WriteLine(string.Format("Sitemap: {0}", Url.AbsoluteUrl("sitemap.xml")));
            return Content(sw.ToString(), "text/plain", Encoding.UTF8);
        }

        [Route("hood/version/")]
        public virtual JsonResult Version()
        {
            return Json(new { version = Engine.Version });
        }


    }
}