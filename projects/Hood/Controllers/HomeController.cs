using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Hood.Web.Controllers
{
    public class HomeController : Hood.Controllers.HomeController
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public HomeController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        [HttpGet]
        [HttpPut]
        public IActionResult Routes()
        {
            var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(x => new {
                Action = x.RouteValues["Action"],
                Controller = x.RouteValues["Controller"],
                Name = x.AttributeRouteInfo?.Name,
                Template = x.AttributeRouteInfo?.Template,
                Contraint = x.ActionConstraints
            }).ToList();
            return View(_actionDescriptorCollectionProvider.ActionDescriptors);
        }

        public override async Task<IActionResult> Index() => await base.Index();


    }
}
