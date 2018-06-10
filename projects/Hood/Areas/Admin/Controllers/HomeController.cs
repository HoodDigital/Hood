using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Hood.Controllers;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor,Manager")]
    public class HomeController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public HomeController()
            : base()
        {
        }

        [Route("admin/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("admin/theme/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Theme()
        {
            return View();
        }

        [Route("admin/stats/")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Stats()
        {
            var content = _content.GetStatistics();
            var users = _auth.GetStatistics();
            var subs = _auth.GetSubscriptionStatistics();
            var properties = _property.GetStatistics();

            return Json(new { content, users, subs, properties });
        }

    }
}
