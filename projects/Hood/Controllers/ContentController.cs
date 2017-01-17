using Hood.Enums;
using Hood.Interfaces;
using Hood.Models;
using Hood.Models.Api;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class ContentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _content;
        private readonly IAuthenticationRepository _auth;
        private readonly ISiteConfiguration _site;
        private readonly IBillingService _billing;
        private readonly ContentCategoryCache _categories;

        public ContentController(
            ContentCategoryCache categories,
            IContentRepository content,
            IAuthenticationRepository auth,
            UserManager<ApplicationUser> userManager,
            IBillingService billing,
            ISiteConfiguration site)
        {
            _categories = categories;
            _userManager = userManager;
            _content = content;
            _auth = auth;
            _billing = billing;
            _site = site;
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
            return View(model);
        }

        [AllowAnonymous]
        public async Task<Response> Tweets()
        {
            SeoSettings _info = _site.GetSeo();
            var tweets = await _content.GetTweets(_info.TwitterHandle.Replace("@", ""), 3);
            Response response = new Response(tweets.ToArray(), 6);
            return response;
        }
    }
}
