using Hood.BaseControllers;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Admin.BaseControllers
{
    public abstract class BaseHomeController : BaseController
    {
        [Route("admin/")]
        public virtual IActionResult Index()
        {
            return View();
        }

        // [Route("admin/stats/")]
        // public virtual async Task<IActionResult> StatsAsync()
        // {
        //     //ContentStatitsics content = new ContentStatitsics();
        //     //UserStatistics users = await _account.GetStatisticsAsync();
        //     //PropertyStatistics properties = await _property.GetStatisticsAsync();
        //     return Json(new Statistics(content, users, properties));
        // }
    }

    public class Statistics
    {
        public Statistics(
            ContentStatitsics content,
            UserStatistics users,
            PropertyStatistics properties
        )
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
