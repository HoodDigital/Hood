using Hood.Core;
using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Hood.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class ThemesController : BaseThemesController
    {
        public ThemesController()
            : base()
        {
        }
    }

    public abstract class BaseThemesController : BaseController
    {
        public BaseThemesController()
            : base()
        {
        }

        [Route("admin/theme/")]
        public virtual IActionResult Index(ThemeListView model) => List(model, "Index");
        [Route("admin/theme/list/")]
        public virtual IActionResult List(ThemeListView model, string viewName = "_List_Themes")
        {
            model.Reload(_themeService.Themes);
            return View(viewName, model);
        }

        [HttpPost()]
        [Route("admin/themes/activate/")]
        public virtual async Task<Response> Activate(string name)
        {
            try
            {
                var applicationLifetime = Engine.Services.Resolve<IHostApplicationLifetime>();
                Engine.Settings.Set(name, "Hood.Settings.Theme");
                applicationLifetime.StopApplication();
                return new Response(true, $"The theme, {name}, has been activated successfully, the app will now restart, this may take a few seconds.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ThemesController>($"Error activating a theme.", ex);
            }
        }

    }
}
