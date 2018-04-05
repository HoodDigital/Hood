using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Models.Payments;
using Hood.Services;
using MailChimp.Net.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class HoodController : Controller
    {
        public readonly IConfiguration _configuration;
        public readonly ISettingsRepository _settings;
        public readonly IHostingEnvironment _environment;
        public readonly IEmailSender _email;
        public readonly IContentRepository _content;
        public readonly IAccountRepository _auth;
        public readonly IConfiguration _config;
        public readonly IHostingEnvironment _env;
        public readonly ContentCategoryCache _categories;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly FormSenderService _forms;

        public HoodController(IAccountRepository auth,
                              ContentCategoryCache categories,
                              UserManager<ApplicationUser> userManager,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISettingsRepository site,
                              IContentRepository content,
                              FormSenderService forms)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _settings = site;
            _categories = categories;
            _userManager = userManager;
            _forms = forms;
        }

        [HttpPost]
        [Route("hood/process-contact-form/")]
        public async Task<Response> ProcessContactForm(ContactFormModel model)
        {
            try
            {
                await _settings.ProcessCaptchaOrThrowAsync(Request);
                return await _forms.ProcessAndSend(model);
            }
            catch (Exception ex)
            {
                return new Models.Response(ex);
            }
        }

        [Route("hood/signup/mailchimp/")]
        public async Task<Response> Mailchimp(MailchimpFormModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IntegrationSettings _integrationSettings = _settings.GetIntegrationSettings();
                    var listId = _integrationSettings.MailchimpListId;
                    var member = new MailChimp.Net.Models.Member { EmailAddress = model.Email, Status = MailChimp.Net.Models.Status.Subscribed };
                    if (model.FirstName.IsSet())
                        member.MergeFields.Add("FNAME", model.FirstName);
                    if (model.LastName.IsSet())
                        member.MergeFields.Add("LNAME", model.LastName);

                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
                    }

                    var client = new HttpClient(handler)
                    {
                        BaseAddress = new Uri($"https://{_integrationSettings.MailchimpDataCenter}.api.mailchimp.com/3.0/lists/")
                    };
                    client.DefaultRequestHeaders.Add("Authorization", _integrationSettings.MailchimpAuthHeader);
                    var memberString = JsonConvert.SerializeObject(
                        member,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }
                    );
                    using (client)
                    {
                        var memberId = member.Id ?? Hash(member.EmailAddress.ToLower());
                        var response = await client.PutAsync(
                            $"{listId}/members/{memberId}",
                            new StringContent(memberString)
                        ).ConfigureAwait(false);
                        await response.EnsureSuccessMailChimpAsync().ConfigureAwait(false);
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    return new Response(true);
                }
                else
                {
                    throw new Exception("There is something wrong with the information you have entered.");
                }
            }
            catch (MailChimpException ex)
            {
                ModelState.AddModelError("Error Saving", ex.Message);
                return new Response(ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error Saving", ex.Message);
                return new Response(ex.Message);
            }
        }
        private string Hash(string emailAddress)
        {
            using (var md5 = MD5.Create()) return md5.GetHash(emailAddress.ToLower());
        }

        [ResponseCache(CacheProfileName = "TenMinutes")]
        [Route("hood/tweets/")]
        public IActionResult TwitterFeed()
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


        [Route("sagepay")]
        public string TestSagePayObject()
        {
            return JsonConvert.SerializeObject(new SagePayTransaction() { UseBilling = true }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

    }
}