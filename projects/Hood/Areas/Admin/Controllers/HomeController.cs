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
    [Authorize(Roles = "SuperUser,Admin,Editor,Manager,SEO")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly IContentRepository _content;
        private readonly ISiteConfiguration _site;
        private readonly IAuthenticationRepository _auth;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;
        private readonly ContentCategoryCache _categories;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IAuthenticationRepository auth,
                              ContentCategoryCache categories,
                              UserManager<ApplicationUser> userManager,
                              IConfiguration conf,
                              IHostingEnvironment env,
                              ISiteConfiguration site,
                              IContentRepository content,
                              IRazorViewRenderer renderer,
                              IEmailSender email)
        {
            _auth = auth;
            _config = conf;
            _env = env;
            _content = content;
            _site = site;
            _renderer = renderer;
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

    }
}
