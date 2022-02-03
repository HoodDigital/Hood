using Hood.Core;
using Hood.Extensions;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hood.Controllers
{
    public class InstallController : Controller
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IConfiguration _config;
        
        public InstallController(IHostApplicationLifetime applicationLifetime, IConfiguration config)
        {
            _applicationLifetime = applicationLifetime;
            _config = config;
        }

        [Route("/install")]
        public IActionResult Install()
        {
            if (Engine.Services.Installed && !User.IsAdminOrBetter())
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new InstallModel()
            {
                DatabaseConfigured = _config.IsDatabaseConnected(),
                DatabaseConnectionFailed = Engine.Services.DatabaseConnectionFailed,
                DatabaseSeedFailed = Engine.Services.DatabaseSeedFailed,
                DatabaseMigrationsMissing = Engine.Services.DatabaseMigrationsMissing,
                MigrationNotApplied = Engine.Services.MigrationNotApplied,
                DatabaseMediaTimeout = Engine.Services.DatabaseMediaTimeout,
                ViewsInstalled = Engine.Services.ViewsInstalled,
                AdminUserSetupError = Engine.Services.AdminUserSetupError,
                Details = Engine.Services.Details
            };
            return View(model);
        }

    }
}
