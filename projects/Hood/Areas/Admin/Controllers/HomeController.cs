using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;
using Hood.Models;
using Microsoft.AspNetCore.Identity;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor,Manager")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly IContentRepository _content;
        private readonly IPropertyRepository _properties;
        private readonly ISettingsRepository _settings;
        private readonly IAccountRepository _auth;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;
        private readonly ContentCategoryCache _categories;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IAccountRepository auth,
                              ContentCategoryCache categories,
                              UserManager<ApplicationUser> userManager,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISettingsRepository settings,
                              IPropertyRepository properties,
                              IContentRepository content,
                              IRazorViewRenderer renderer,
                              IEmailSender email)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _settings = settings;
            _renderer = renderer;
            _properties = properties;
            _email = email;
            _categories = categories;
            _userManager = userManager;
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
            var properties = _properties.GetStatistics();

            return Json(new { content, users, subs, properties });
        }

    }
}
