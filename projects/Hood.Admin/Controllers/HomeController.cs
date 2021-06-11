using Hood.Controllers;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            ApplicationUser user = await _userManager.GetUserAsync(User);
            user.AddUserNote(new UserNote()
            {
                Id = Guid.NewGuid(),
                CreatedBy = user.Id,
                CreatedOn = DateTime.UtcNow,
                Note = "This account was loaded and checked via the debug page."
            });
            await _userManager.UpdateAsync(user);

            return View();
        }

        [Route("admin/stats/")]
        public async Task<IActionResult> StatsAsync()
        {
            ContentStatitsics content = await _content.GetStatisticsAsync();
            UserStatistics users = await _account.GetStatisticsAsync();
            PropertyStatistics properties = await _property.GetStatisticsAsync();
            return Json(new Statistics(content, users, properties));
        }

    }
    public class Statistics
    {
        public Statistics(ContentStatitsics content, UserStatistics users, PropertyStatistics properties)
        {
            Content = content;
            Users = users;
            Properties = properties;
        }

        public ContentStatitsics Content { get; set; }
        public UserStatistics Users { get; set; }
        public PropertyStatistics Properties { get; set; }
    }

}
