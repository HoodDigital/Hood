using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using MailChimp.Net.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    [Area("Hood")]
    public class HomeController : Controller
    {
        public readonly IConfiguration _configuration;
        public readonly ISettingsRepository _settings;
        public readonly IHostingEnvironment _environment;
        public readonly IEmailSender _email;
        public readonly IContentRepository _content;
        private readonly IAccountRepository _auth;
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly ContentCategoryCache _categories;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFormSenderService _forms;

        public HomeController(IAccountRepository auth,
                              ContentCategoryCache categories,
                              UserManager<ApplicationUser> userManager,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISettingsRepository site,
                              IContentRepository content,
                              IFormSenderService forms)
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

        #region "Content"

        [ResponseCache(CacheProfileName = "Day")]
        public IActionResult Index()
        {
            BasicSettings settings = _settings.GetBasicSettings();
            if (settings.LockoutMode && ControllerContext.HttpContext.IsLockedOut(_settings.LockoutAccessCodes))
            {
                if (settings.LockoutModeHoldingPage.HasValue)
                    return Show(settings.LockoutModeHoldingPage.Value);
                else
                    return View("~/Views/Home/Holding.cshtml");
            }
            else
            {
                if (settings.Homepage.HasValue)
                    return Show(settings.Homepage.Value);
                else
                    return View();
            }
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
            ContentListModel model = new ContentListModel()
            {
                Posts = content,
                Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, type),
                Type = _settings.GetContentSettings().GetContentType(type)
            };
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
            ContentListModel model = new ContentListModel()
            {
                Posts = content,
                Type = _settings.GetContentSettings().GetContentType(type)
            };
            if (!model.Type.Enabled || !model.Type.IsPublic)
                return NotFound();
            model.Recent = _content.GetPagedContent(new ListFilters() { page = 1, pageSize = 5, sort = "PublishDateDesc" }, model.Type.Type);
            model.Search = filters.search;
            model.Author = _settings.ToApplicationUserApi(_auth.GetUserById(author));
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
            ContentListModel model = new ContentListModel()
            {
                Posts = content,
                Type = _settings.GetContentSettings().GetContentType(type)
            };
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
            if (id == 0)
                return NotFound();

            ContentModel model = new ContentModel()
            {
                EditMode = editMode,
                Content = _content.GetContentByID(id)
            };
            if (model.Content == null)
                return NotFound();

            model.Type = _settings.GetContentSettings().GetContentType(model.Content.ContentType);

            if (model.Type == null || !model.Type.Enabled || !model.Type.HasPage)
                return NotFound();

            ContentNeighbours cn = _content.GetNeighbourContent(id, model.Type.Type);
            model.Previous = cn.Previous;
            model.Next = cn.Next;

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && model.Content.Status != (int)Status.Published)
                return NotFound();

            if (model.Type.BaseName == "Page")
            {
                // if admin only, and not logged in as admin hide.
                if (model.Content.GetMeta("Settings.Security.AdminOnly") != null)
                    if (!User.IsInRole("Admin") && model.Content.GetMeta("Settings.Security.AdminOnly").Get<bool>() == true)
                        return NotFound();

                // if not public, and not logged in hide.
                if (model.Content.GetMeta("Settings.Security.Public") != null)
                    if (!User.Identity.IsAuthenticated && model.Content.GetMeta("Settings.Security.Public").Get<bool>() == false)
                        return NotFound();

                var result = _settings.SubscriptionsEnabled();
                if (result.Succeeded)
                {
                    if (model.Content.GetMeta("Settings.Security.Subscription").IsStored)
                    {
                        List<string> subs = JsonConvert.DeserializeObject<List<string>>(model.Content.GetMeta("Settings.Security.Subscription").Get<string>());
                        if (subs != null)
                            if (subs.Count > 0)
                            {
                                AccountInfo subscribeResult = ControllerContext.HttpContext.GetAccountInfo();
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
            SeoSettings _info = _settings.GetSeo();
            var tweets = await _content.GetTweets(_info.TwitterHandle.Replace("@", ""), 3);
            Response response = new Response(tweets.ToArray(), 6);
            return response;
        }

        #endregion

        [HttpPost]
        [Route("hood/process-contact-form/")]
        public async Task<Response> ProcessContactForm(ContactFormModel model)
        {
            try
            {
                await _settings.ProcessCaptchaOrThrowAsync(Request);
                return await _forms.ProcessContactFormModel(model);
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
                        var responseContent = await response.Content.ReadAsAsync<MailChimp.Net.Models.Member>().ConfigureAwait(false);
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

        [Route("account/access-denied/")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
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
    }
}
