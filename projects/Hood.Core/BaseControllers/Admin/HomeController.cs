using System.Threading.Tasks;
using Hood.BaseControllers;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.Admin.BaseControllers
{
    public abstract class BaseHomeController : BaseController
    {
        [Route("admin/")]
        public virtual IActionResult Index()
        {
            return View();
        }

        [Route("admin/stats/")]
        public virtual async Task<IActionResult> StatsAsync()
        {
            var content = await HttpContext
                .RequestServices.GetRequiredService<IContentRepository>()
                .GetStatisticsAsync();

            IPasswordAccountRepository passwordAccount =
                HttpContext.RequestServices.GetService<IPasswordAccountRepository>();
            IAuth0AccountRepository auth0Account =
                HttpContext.RequestServices.GetService<IAuth0AccountRepository>();

            UserStatistics users =
                passwordAccount != null ? await passwordAccount.GetStatisticsAsync()
                : auth0Account != null ? await auth0Account.GetStatisticsAsync()
                : null;

            var propertyRepo = HttpContext.RequestServices.GetService<IPropertyRepository>();
            PropertyStatistics properties =
                propertyRepo != null ? await propertyRepo.GetStatisticsAsync() : null;

            return Json(new Statistics(content, users, properties));
        }
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
