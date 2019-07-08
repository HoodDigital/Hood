using Hood.Core;
using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public Response Activate(string name)
        {
            try
            {
                Engine.Settings.Set("Hood.Settings.Theme", name);
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

    }
}
