using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public abstract class BaseHomeController : BaseHomeController<HoodDbContext>
    {
        public BaseHomeController() : base() { }
    }

    public abstract class BaseHomeController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {

        public BaseHomeController() : base() { }

        [ResponseCache(CacheProfileName = "Day")]
        public virtual async Task<IActionResult> Index()
        {

            BasicSettings basicSettings = _settings.GetBasicSettings();

            if (basicSettings.LockoutMode && ControllerContext.HttpContext.IsLockedOut(_settings.LockoutAccessCodes))
            {
                if (basicSettings.LockoutModeHoldingPage.HasValue)
                    return await Show(basicSettings.LockoutModeHoldingPage.Value);
                else
                    return View(nameof(Holding));
            }
            else
            {
                if (basicSettings.Homepage.HasValue)
                    return await Show(basicSettings.Homepage.Value);
                else
                    return View();
            }
        }

        public virtual IActionResult Holding()
        {
            return View();
        }

        #region "Content"

        [ResponseCache(CacheProfileName = "Hour")]
        public virtual async Task<IActionResult> Feed(ContentModel model, string type)
        {
            model.Type = type;
            model.ContentType = _settings.GetContentSettings().GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();

            model = await _content.GetPagedContent(model, true);
            model.Recent = await _content.GetPagedContent(new ContentModel() { Type = type, PageIndex = 1, PageSize = 5, Order = "PublishDateDesc" }, true);
            return View("Feed", model);
        }

        // Add your custom actions and site functionality here, or create new controllers, do whatever!
        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/load-more/")]
        public async Task<IActionResult> LoadMore(string type, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
        {
            ContentModel model = new ContentModel()
            {
                Search = search,
                Order = sort,
                PageIndex = page,
                PageSize = size
            };
            model = await _content.GetPagedContent(model, true);
            model.ContentType = _settings.GetContentSettings().GetContentType(type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();
            return View("LoadMore", model);
        }

        [ResponseCache(CacheProfileName = "Day")]
        public virtual async Task<IActionResult> Show(int id, bool editMode = false)
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

            model.Type = model.Content.ContentType;
            model.ContentType = _settings.GetContentSettings().GetContentType(model.Content.ContentType);

            if (model.Type == null || !model.ContentType.Enabled || !model.ContentType.HasPage)
                return NotFound();

            ContentNeighbours cn = _content.GetNeighbourContent(id, model.ContentType.Type);
            model.Previous = cn.Previous;
            model.Next = cn.Next;

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && model.Content.Status != (int)Status.Published)
                return NotFound();

            if (model.ContentType.BaseName == "Page")
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

            model.Recent = await _content.GetPagedContent(new ContentModel() { Type = model.Type, PageIndex = 1, PageSize = 5, Order = "PublishDateDesc" }, true);

            foreach (ContentMeta cm in model.Content.Metadata)
            {
                ViewData[cm.Name] = cm.GetStringValue();
            }
            return View("Show", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/author/{author}/")]
        public virtual async Task<IActionResult> Author(ContentModel model, string type)
        {
            model.ContentType = _settings.GetContentSettings().GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();

            model = await _content.GetPagedContent(model, true);
            model.Recent = await _content.GetPagedContent(new ContentModel() { Type = type, PageIndex = 1, PageSize = 5, Order = "PublishDateDesc" }, true);
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/category/{category}/")]
        public virtual async Task<IActionResult> Category(ContentModel model, string type)
        {
            model.ContentType = _settings.GetContentSettings().GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();

            model = await _content.GetPagedContent(model, true);
            model.Recent = await _content.GetPagedContent(new ContentModel() { Type = type, PageIndex = 1, PageSize = 5, Order = "PublishDateDesc" }, true);
            return View("Feed", model);
        }

        #endregion



    }
}