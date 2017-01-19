using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Models.Api;
using Hood.Services;
using MailChimp.Net;
using MailChimp.Net.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly IRazorViewRenderer _renderer;
        private readonly IAuthenticationRepository _auth;
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly ContentCategoryCache _categories;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IAuthenticationRepository auth,
                              ContentCategoryCache categories,
                              UserManager<ApplicationUser> userManager,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISiteConfiguration site,
                              IContentRepository content,
                              IRazorViewRenderer renderer,
                              IEmailSender email)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _site = site;
            _renderer = renderer;
            _email = email;
            _categories = categories;
            _userManager = userManager;
        }
        #region "Content"

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Index()
        {
            BasicSettings settings = _site.GetBasicSettings();
            if (settings.Homepage.HasValue)
                return Show(settings.Homepage.Value);
            else
                return View();
        }

        [ResponseCache(CacheProfileName = "Hour")]
        public IActionResult Feed(string type, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
        {
            ListFilters filters = new ListFilters()
            {
                search = search,
                sort = sort,
                page = page,
                pageSize = size
            };
            PagedList<Content> content = _content.GetPagedContent(filters, type, null, null, null, true);
            ContentListModel model = new ContentListModel();
            model.Posts = content;
            model.Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, type);
            model.Type = _content.GetContentType(type);
            if (!model.Type.Enabled || !model.Type.IsPublic)
                return NotFound();
            model.Search = filters.search;
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/author/{author}/")]
        public IActionResult Author(string author, string type, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
        {
            ListFilters filters = new ListFilters()
            {
                search = search,
                sort = sort,
                page = page,
                pageSize = size
            };
            PagedList<Content> content = _content.GetPagedContent(filters, type, null, author, null, true);
            ContentListModel model = new ContentListModel();
            model.Posts = content;
            model.Type = _content.GetContentType(type);
            if (!model.Type.Enabled || !model.Type.IsPublic)
                return NotFound();
            model.Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, model.Type.Type);
            model.Search = filters.search;
            model.Author = new ApplicationUserApi(_auth.GetUserById(author));
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/category/{category}/")]
        public IActionResult Category(string type, string category, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
        {
            ListFilters filters = new ListFilters()
            {
                search = search,
                sort = sort,
                page = page,
                pageSize = size
            };
            PagedList<Content> content = _content.GetPagedContent(filters, type, category, null, null, true);
            ContentListModel model = new ContentListModel();
            model.Posts = content;
            model.Type = _content.GetContentType(type);
            if (model.Type == null || !model.Type.Enabled || !model.Type.IsPublic)
                return NotFound();
            model.Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, model.Type.Type);
            model.Search = filters.search;
            model.Category = _categories.FromSlug(model.Type.Type, category).DisplayName;
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Show(int id, bool editMode = false)
        {
            ContentModel model = new ContentModel();
            model.EditMode = editMode;
            model.Content = _content.GetContentByID(id);
            model.Type = _content.GetContentType(model.Content.ContentType);

            if (model.Type == null || !model.Type.Enabled || !model.Type.HasPage)
                return NotFound();

            ContentNeighbours cn = _content.GetNeighbourContent(id, model.Type.Type);
            model.Previous = cn.Previous;
            model.Next = cn.Next;

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && model.Content.Status != (int)Status.Published)
                return NotFound();

            if (model.Type.Type == "page")
            {
                // if admin only, and not logged in as admin hide.
                if (model.Content.GetMeta("Settings.Security.AdminOnly") != null)
                    if (!User.IsInRole("Admin") && model.Content.GetMeta("Settings.Security.AdminOnly").Get<bool>() == true)
                        return NotFound();

                // if not public, and not logged in hide.
                if (model.Content.GetMeta("Settings.Security.Public") != null)
                    if (!User.Identity.IsAuthenticated && model.Content.GetMeta("Settings.Security.Public").Get<bool>() == false)
                        return NotFound();

                var result = _site.SubscriptionsEnabled();
                if (result.Succeeded)
                {
                    if (model.Content.GetMeta("Settings.Security.Subscription").IsStored)
                    {
                        List<string> subs = JsonConvert.DeserializeObject<List<string>>(model.Content.GetMeta("Settings.Security.Subscription").Get<string>());
                        if (subs != null)
                            if (subs.Count > 0)
                            {
                                AccountInfo subscribeResult = _auth.GetAccountInfo(_userManager.GetUserId(User));
                                if (!subs.Any(s => subscribeResult.IsSubscribed(s)))
                                    return RedirectToAction("Index", "Subscriptions", new { message = BillingMessage.UpgradeRequired });
                            }
                    }
                }
            }

            model.Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, model.Type.Type);

            foreach (ContentMeta cm in model.Content.Metadata)
            {
                ViewData[cm.Name] = cm.GetStringValue();
            }
            return View("Show", model);
        }

        public async Task<Response> Tweets()
        {
            SeoSettings _info = _site.GetSeo();
            var tweets = await _content.GetTweets(_info.TwitterHandle.Replace("@", ""), 3);
            Response response = new Response(tweets.ToArray(), 6);
            return response;
        }

        #endregion



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
                    var member = new MailChimp.Net.Models.Member { EmailAddress = model.Email, Status = MailChimp.Net.Models.Status.Subscribed };
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
