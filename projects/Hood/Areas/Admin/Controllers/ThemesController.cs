using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ThemesController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
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
                // set the site theme
                bool res = _settings.Set("Hood.Settings.Theme", name);
                if (res)
                {
                    return new Response(true);
                }
                else
                {
                    throw new Exception("The database could not be updated, please try later.");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

    }
}
