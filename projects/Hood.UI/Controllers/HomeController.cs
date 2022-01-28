using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
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

        [Route("/")]
        public virtual async Task<IActionResult> Index()
        {
            var service = new Auth0Service();
            // var roles = await service.GetRoles();
            var users = await service.GetUsers("admin");
            // var user = await service.GetUserByEmail("admin@hooddigital.com");     

            var userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();
            var dood = await userManager.GetUsersInRoleAsync("Admin");       

            if (User.Identity.IsAuthenticated) {
                //do sommat
            }

            BasicSettings basicSettings = Engine.Settings.Basic;

            if (basicSettings.Homepage.HasValue)
                return await Show(basicSettings.Homepage.Value);
            else
                return View();
        }

        public virtual IActionResult Holding()
        {
            return View();
        }

        #region "Content"

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

        [Route("{type:contentTypeSlug}/{id:int}/{title?}")]
        [Route("{url:pageSlug}/{urlSlug1?}/{urlSlug2?}/{urlSlug3?}/{urlSlug4?}", Order = 2)]
        [Route("content/show/{id}", Order = 3)]
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