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
    public abstract class BaseHomeController : Controller
    {
        public IAccountRepository _auth { get; }
        public ISettingsRepository _settings { get; }
        public IContentRepository _content { get; }
        public ContentCategoryCache _categories { get; }

        public BaseHomeController(IAccountRepository auth,
                              IContentRepository content,
                              ContentCategoryCache categories, 
                              ISettingsRepository settings)
        {
            _auth = auth;
            _settings = settings;
            _content = content;
            _categories = categories;
        }

        [ResponseCache(CacheProfileName = "Day")]
        public virtual IActionResult Index()
        {
            BasicSettings basicSettings = _settings.GetBasicSettings();

            if (basicSettings.LockoutMode && ControllerContext.HttpContext.IsLockedOut(_settings.LockoutAccessCodes))
            {
                if (basicSettings.LockoutModeHoldingPage.HasValue)
                    return Show(basicSettings.LockoutModeHoldingPage.Value);
                else
                    return View("~/Views/Home/Holding.cshtml");
            }
            else
            {
                if (basicSettings.Homepage.HasValue)
                    return Show(basicSettings.Homepage.Value);
                else
                    return View();
            }
        }

        #region "Content"

        [ResponseCache(CacheProfileName = "Hour")]
        public virtual IActionResult Feed(string type, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
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

        [ResponseCache(CacheProfileName = "Day")]
        public virtual IActionResult Show(int id, bool editMode = false)
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

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/author/{author}/")]
        public virtual IActionResult Author(string author, string type, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
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
            model.Author = _auth.GetUserById(author);
            return View("Feed", model);
        }

        [ResponseCache(CacheProfileName = "Hour")]
        [Route("{type}/category/{category}/")]
        public virtual IActionResult Category(string type, string category, string search, string sort = "PublishDateDesc", int page = 1, int size = 12)
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

        #endregion

    }
}