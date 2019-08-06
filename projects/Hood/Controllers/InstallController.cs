using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Hood.Controllers
{
    public class InstallController : Controller
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly IConfiguration _config;
        public InstallController(IApplicationLifetime applicationLifetime, IConfiguration config)
        {
            _applicationLifetime = applicationLifetime;
            _config = config;
        }

        public IActionResult Install()
        {
            var model = new InstallModel();
            var context = Engine.Services.Resolve<HoodDbContext>();
            model.DatabaseConfigured = _config.IsDatabaseConfigured();
            try
            {
                var profile = context.UserProfiles.FirstOrDefault();
                model.ViewsInstalled = true;
            }
            catch (Exception)
            {
                model.ViewsInstalled = false;
            }
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
