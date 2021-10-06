using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Hood.Web.Controllers
{
    public class HomeController : Hood.Controllers.HomeController
    {        
        public HomeController() : base() { }
        public override async Task<IActionResult> Index() => await base.Index();
    }
}
