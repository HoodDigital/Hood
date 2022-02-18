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
            if (Engine.Services.Installed)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Route("/install/ready")]
        public IActionResult Initialized()
        {
            return View();
        }

    }
}
