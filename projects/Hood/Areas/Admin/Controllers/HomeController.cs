using Hood.Controllers;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor")]
    public class HomeController : BaseController
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

        [Route("admin/debug")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> Debug()
        {
            var user = await _userManager.GetUserAsync(User);
            user.AddUserNote(new UserNote()
            {
                Id = Guid.NewGuid(),
                CreatedBy = user.Id,
                CreatedOn = DateTime.Now,
                Note = "This account was loaded and checked via the debug page."
            });
            await _userManager.UpdateAsync(user);

            return View();
        }

        [Route("admin/stats/")]
        public async Task<IActionResult> StatsAsync()
        {
            var content = await _content.GetStatisticsAsync();
            var users = await _account.GetStatisticsAsync();
            var subs = await _account.GetSubscriptionStatisticsAsync();
            var properties = await _property.GetStatisticsAsync();

            return Json(new { content, users, subs, properties });
        }

    }
}
