using Hood.Core;
using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class ThemesController : BaseController
    {
        public ThemesController()
            : base()
        {
        }

        [Route("admin/theme/")]
        public IActionResult Index() => List("Index");
        [Route("admin/theme/list/")]
        public IActionResult List(string viewName = "_List_Themes")
        {
            return View(viewName);
        }

        [HttpPost()]
        [Route("admin/themes/activate/")]
        public async Task<Response> Activate(string name)
        {
            try
            {
                Engine.Settings.Set(name, "Hood.Settings.Theme");
                return new Response(true, $"The theme, {name}, has been activated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ThemesController>($"Error activating a theme.", ex);
            }
        }

    }
}
