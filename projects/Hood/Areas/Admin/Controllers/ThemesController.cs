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

        [HttpPost()]
        [Route("admin/themes/activate/")]
        public async Task<Response> Activate(string name)
        {
            try
            {
                Engine.Settings.Set("Hood.Settings.Theme", name);
#warning TODO: Handle response in JS.
                return new Response(true, $"The theme, {name}, has been activated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ThemesController>($"Error activating a theme.", ex);
            }
        }

    }
}
