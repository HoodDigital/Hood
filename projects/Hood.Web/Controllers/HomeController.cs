using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Hood.ViewModels;

namespace Hood.Controllers
{
    public class HomeController : BaseHomeController
    {
        public HomeController(IAccountRepository auth, IContentRepository content, ContentCategoryCache categories, ISettingsRepository settings) 
            : base(auth, content, categories, settings)
        {
        }

        public override IActionResult Index()
        {
            return base.View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
