using System.Diagnostics;
using System.Threading.Tasks;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using Hood.Web;

namespace Hood.Controllers
{
    public class HomeController : BaseHomeController
    {
        public HomeController()
            : base()
        {}

        public override async Task<IActionResult> Index() => await base.Index();

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
