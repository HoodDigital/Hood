using Hood.Core;
using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
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
                return new Response(true);
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while activating a theme: {ex.Message}";
                await _logService.AddExceptionAsync<ThemesController>(SaveMessage, ex);
                return new Response(SaveMessage);
            }
        }

    }
}
