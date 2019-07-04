using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(Roles = "SuperUser,Admin")]
        public IActionResult Theme()
        {
            return View();
        }

        [Route("admin/stats/")]
        public IActionResult Stats()
        {
            var content = _content.GetStatistics();
            var users = _account.GetStatistics();
            var subs = _account.GetSubscriptionStatistics();
            var properties = _property.GetStatistics();

            return Json(new { content, users, subs, properties });
        }

    }
}
