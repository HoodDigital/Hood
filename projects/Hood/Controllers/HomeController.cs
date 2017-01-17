using Hood.Extensions;
using Hood.Models;
using Hood.Models.Api;
using Hood.Services;
using MailChimp.Net;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class HomeController : Controller
    {
        public readonly IConfiguration _configuration;
        public readonly ISiteConfiguration _site;
        public readonly IHostingEnvironment _environment;
        public readonly IEmailSender _email;
        public readonly IContentRepository _content;
        public readonly IMemoryCache _cache;
        public readonly ILogger _logger;

        public HomeController(IConfiguration conf, ISiteConfiguration site, IContentRepository content, IHostingEnvironment env, IEmailSender email, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            _configuration = conf;
            _content = content;
            _site = site;
            _environment = env;
            _email = email;
            _cache = cache;
            _logger = loggerFactory.CreateLogger(typeof(HomeController));
        }

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Index()
        {
            HomePageModel model = new HomePageModel();
            model.News = _content.GetRecent("news").Select(c => new ContentApi(c)).ToList();
            model.Testimonial = _content.GetRecent("testimonial").Select(c => new ContentApi(c)).ToList();
            return View(model);
        }

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Holding()
        {
            return View(new ContactFormModel());
        }

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Maintenance()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = "Month")]
        [Route("terms/")]
        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(CacheProfileName = "Month")]
        [Route("privacy/")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [Route("contact/")]
        public async Task<Response> Contact(IContactFormModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ContactSettings contactSettings = _site.GetContactSettings();

                    try
                    {
                        MailObject message = new MailObject();

                        string siteEmail = contactSettings.Email;
                        if (!string.IsNullOrEmpty(siteEmail))
                        {
                            message.To = new SendGrid.Helpers.Mail.Email(siteEmail);
                            message.PreHeader = "New enquiry via the " + _site.GetSiteTitle() + " website.";
                            message.Subject = "New enquiry via the " + _site.GetSiteTitle() + " website.";
                            message.AddH1("New enquiry!");
                            message.AddParagraph("A new enquiry has been recieved via " + _site.GetSiteTitle() + " website.");
                            message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                            message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                            message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                            message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                            message.AddParagraph("Enquiry:");
                            message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                            await _email.SendEmail(message);
                        }

                        string msg = contactSettings.ThankYouMessage;
                        if (string.IsNullOrEmpty(msg))
                            msg += "Thank you for contacting us! Your enquiry has been successfully sent, and we are currently digesting it. We will be in touch once we have had a read. Thanks!";

                        message = new MailObject();
                        message.To = new SendGrid.Helpers.Mail.Email(model.Email);
                        message.PreHeader = "Your enquiry with " + _site.GetSiteTitle();
                        message.Subject = "Your enquiry with " + _site.GetSiteTitle();
                        message.AddH1("Your enquiry has been sent.");
                        message.AddParagraph(msg);
                        message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                        message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                        message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                        message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                        message.AddParagraph("Enquiry:");
                        message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                        await _email.SendEmail(message);

                        return new Response(true);
                    }
                    catch (Exception sendEx)
                    {
                        _logger.LogWarning((int)ErrorEvent.Email, sendEx, "Problem sending email from HomeController.DoContact()");
                        throw new Exception("There was a problem sending the message: " + sendEx.Message);
                    }

                }
                else
                {
                    throw new Exception("There is something wrong with the information you have entered.");
                }

            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        [Route("signup/mailchimp/")]
        public async Task<Response> Mailchimp(MailchimpFormModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IntegrationSettings _settings = _site.GetIntegrationSettings();
                    IMailChimpManager manager = new MailChimpManager(_settings.MailchimpApiKey); //if you have it in code
                    var listId = _settings.MailchimpListId;
                    var member = new Member { EmailAddress = model.Email, Status = MailChimp.Net.Models.Status.Subscribed };
                    if (model.FirstName.IsSet())
                        member.MergeFields.Add("FNAME", model.FirstName);
                    if (model.LastName.IsSet())
                        member.MergeFields.Add("LNAME", model.LastName);
                    await manager.Members.AddOrUpdateAsync(listId, member);
                    return new Response(true);
                }
                else
                {
                    throw new Exception("There is something wrong with the information you have entered.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error Saving", ex.Message);
                return new Response(ex.Message);
            }
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
            foreach (ContentType ct in _content.GetRestrictedTypes())
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
            return this.Content(xml, "text/xml", Encoding.UTF8);
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
            _logger.LogWarning((int)ErrorEvent.General, "Error " + code.ToString() + ": " + Request.Path.ToUriComponent().ToString());

            switch (code)
            {
                case 404:
                    ViewData["Title"] = "Gone!";
                    ViewData["Message"] = "Unfortunately, we can't locate what you are looking for...";
                    break;
            }

            return View("~/Views/Shared/ErrorCode.cshtml");
        }

        [Route("account/access-denied/")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }
    }
}
