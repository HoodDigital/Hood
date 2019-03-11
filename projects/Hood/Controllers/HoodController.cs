using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class HoodController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public HoodController()
            : base()
        { }

        [HttpPost]
        [Route("hood/contact/send/")]
        [Route("hood/process-contact-form/")]
        public async Task<Response> ProcessContactForm(ContactFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new Response("The submitted information is not valid.");

                if (model.IsSpambot)
                    return new Response("You have been flagged as a spam bot. If this is not true, please contact us via email.");

                await _settings.ProcessCaptchaOrThrowAsync(Request);

                var contactSettings = _settings.GetContactSettings();

                model.SendToRecipient = true;
                model.NotifyRole = "ContactFormNotifications";

                return await _mailService.ProcessAndSend(model);
            }
            catch (Exception ex)
            {
                return new Models.Response(ex);
            }
        }

        [ResponseCache(CacheProfileName = "TenMinutes")]
        [Route("hood/tweets/")]
        public IActionResult TwitterFeed()
        {
            return View();
        }

        [Route("robots.txt")]
        public IActionResult Robots()
        {
            var sw = new StringWriter();
            //write the header
            sw.WriteLine("User-agent: *");
            sw.WriteLine("Disallow: /admin/ ");
            sw.WriteLine("Disallow: /account/ ");
            sw.WriteLine("Disallow: /manage/ ");
            sw.WriteLine("Disallow: /install/ ");
            foreach (ContentType ct in _settings.GetContentSettings().GetRestrictedTypes())
            {
                sw.WriteLine("Disallow: /" + ct.Slug + "/ ");
            }
            sw.WriteLine(string.Format("Sitemap: {0}", Url.AbsoluteUrl("sitemap.xml")));
            return Content(sw.ToString(), "text/plain", Encoding.UTF8);
        }

        [Route("sitemap.xml")]
        public ActionResult SitemapXml()
        {
            string xml = _content.GetSitemapDocument(Url);
            return Content(xml, "text/xml", Encoding.UTF8);
        }

        [Route("error/")]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;
            return View("~/Views/Shared/Error.cshtml", error);
        }

        [Route("error/{code}")]
        public IActionResult ErrorCode(int code)
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;

            switch (code)
            {
                case 404:
                    ViewData["Title"] = "Gone!";
                    ViewData["Message"] = "Unfortunately, we can't locate what you are looking for...";
                    break;
            }

            return View("~/Views/Shared/ErrorCode.cshtml");
        }

        [Route("enter-access-code")]
        public IActionResult LockoutModeEntrance(string returnUrl)
        {
            if (ControllerContext.HttpContext.Session.TryGetValue("LockoutModeToken", out byte[] betaCodeBytes))
            {
                ViewData["token"] = Encoding.Default.GetString(betaCodeBytes);
                ViewData["error"] = "The token you have entered is not valid.";
            }
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [Route("enter-access-code")]
        [HttpPost]
        public IActionResult LockoutModeEntrance(string token, string returnUrl)
        {
            if (token.IsSet())
            {
                ControllerContext.HttpContext.Session.Set("LockoutModeToken", Encoding.ASCII.GetBytes(token));
                return Redirect(returnUrl);
            }
            ViewData["returnUrl"] = returnUrl;
            ViewData["token"] = token;
            ViewData["error"] = "The token you have entered is not valid.";
            return View();
        }

        [Route("hood/version/")]
        public JsonResult GetVersion()
        {
            return Json(new { version = _settings.GetVersion() });
        }
    }
}