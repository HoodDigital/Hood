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

        public IActionResult Install()
        {
            var model = new InstallModel()
            {
                DatabaseConfigured = _config.IsDatabaseConnected(),
                DatabaseConnectionFailed = Engine.Services.DatabaseConnectionFailed,
                DatabaseSeedFailed = Engine.Services.DatabaseSeedFailed,
                DatabaseMigrationsMissing = Engine.Services.DatabaseMigrationsMissing,
                MigrationNotApplied = Engine.Services.MigrationNotApplied,
                DatabaseMediaTimeout = Engine.Services.DatabaseMediaTimeout,
                ViewsInstalled = Engine.Services.ViewsInstalled,
                AdminUserSetupError = Engine.Services.AdminUserSetupError
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public IActionResult Install(string reason)
        {
            if (!_config.IsDatabaseConfigured())
                _applicationLifetime.StopApplication();

            ViewData["Restarting"] = "App is restarting... please do not refresh the page.";
            return View();
        }
    }
}
