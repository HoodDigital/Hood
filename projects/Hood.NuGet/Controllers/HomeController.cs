using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Hood.NuGet.Controllers
{
    public class HomeController : Hood.Controllers.HomeController
    {
        public HomeController() : base()
        {}

        public override async Task<IActionResult> Index() => await base.Index();
    }
}
