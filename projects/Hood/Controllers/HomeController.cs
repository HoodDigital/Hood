using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
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
    public abstract class HomeController : HomeController<HoodDbContext>
    {
        public HomeController() : base() { }
    }

    public abstract class HomeController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {
        public HomeController() : base() { }

        [ResponseCache(CacheProfileName = "Day")]
        [Route("/")]
        public virtual async Task<IActionResult> Index()
        {

            BasicSettings basicSettings = Engine.Settings.Basic;

            if (basicSettings.LockoutMode && ControllerContext.HttpContext.IsLockedOut(Engine.Settings.LockoutAccessCodes))
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
        [Route("{type:contentTypeSlug}/")]
        public virtual async Task<IActionResult> Feed(ContentModel model)
        {
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();

            model.Status = ContentStatus.Published;
            model = await _content.GetContentAsync(model);
            model.Recent = await _content.GetRecentAsync(model.Type);

            if (model.Inline)
                return View("_Inline_Feed", model);

            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type:contentTypeSlug}/author/{author}/")]
        public virtual async Task<IActionResult> Author(ContentModel model)
        {
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();
            model.Status = ContentStatus.Published;
            model = await _content.GetContentAsync(model);
            model.Recent = await _content.GetRecentAsync(model.Type);
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type:contentTypeSlug}/category/{category}/")]
        public virtual async Task<IActionResult> Category(ContentModel model)
        {
            model.ContentType = Engine.Settings.Content.GetContentType(model.Type);
            if (!model.ContentType.Enabled || !model.ContentType.IsPublic)
                return NotFound();

            model.Status = ContentStatus.Published;
            model = await _content.GetContentAsync(model);
            model.Recent = await _content.GetRecentAsync(model.Type, model.Category);
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Day")]
        [Route("{type:contentTypeSlug}/{id:int}/{title?}")]
        [Route("{url:pageSlug}/{urlSlug1?}/{urlSlug2?}/{urlSlug3?}/{urlSlug4?}", Order = 2)]
        public virtual async Task<IActionResult> Show(int id, bool editMode = false)
        {
            if (id == 0)
                return NotFound();

            ContentModel model = new ContentModel()
            {
                EditMode = editMode,
                Content = await _content.GetContentByIdAsync(id)
            };
            if (model.Content == null)
                return NotFound();

            model.Type = model.Content.ContentType;
            model.ContentType = Engine.Settings.Content.GetContentType(model.Content.ContentType);

            if (model.Type == null || !model.ContentType.Enabled || !model.ContentType.HasPage)
                return NotFound();

            ContentNeighbours cn = await _content.GetNeighbourContentAsync(id, model.ContentType.Type);
            model.Previous = cn.Previous;
            model.Next = cn.Next;

            // if not admin, and not published, hide.
            if (!(User.IsEditorOrBetter()) && model.Content.Status != ContentStatus.Published)
                return NotFound();

            if (model.ContentType.BaseName == "Page")
            {
                // if admin only, and not logged in as admin hide.
                if (model.Content.GetMeta("Settings.Security.AdminOnly") != null)
                    if (!User.IsAdminOrBetter() && model.Content.GetMetaValue<bool>("Settings.Security.AdminOnly") == true)
                        return NotFound();

                // if not public, and not logged in hide.
                if (model.Content.GetMeta("Settings.Security.Public") != null)
                    if (!User.Identity.IsAuthenticated && model.Content.GetMetaValue<bool>("Settings.Security.Public") == false)
                        return NotFound();

                if (Engine.Settings.Billing.IsSubscriptionsEnabled)
                {
                    if (model.Content.GetMeta("Settings.Security.Subscription") != null)
                    {
                        List<string> subs = JsonConvert.DeserializeObject<List<string>>(model.Content.GetMetaValue<string>("Settings.Security.Subscription"));
                        if (subs != null)
                            if (subs.Count > 0)
                            {
                                if (!subs.Any(s => User.IsSubscribed(s)))
                                {
                                    SaveMessage = "You need to purchase, upgrade or change your package to view this.";
                                    MessageType = AlertType.Warning;
                                    return RedirectToAction("Index", "Subscriptions");
                                }
                            }
                    }
                }
            }

            model.Recent = await _content.GetRecentAsync(model.Type, model.Category);

            foreach (ContentMeta cm in model.Content.Metadata)
            {
                ViewData[cm.Name] = cm.GetStringValue();
            }
            return View("Show", model);
        }

        #endregion
    }
}