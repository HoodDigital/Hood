using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Controllers
{
    public class PropertyController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _property;
        private readonly ISiteConfiguration _site;
        private readonly IHostingEnvironment _env;
        private readonly IBillingService _billing;

        public PropertyController(
            IPropertyRepository property,
            UserManager<ApplicationUser> userManager,
            ISiteConfiguration site,
            IBillingService billing,
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _property = property;
            _site = site;
            _billing = billing;
            _env = env;
        }

        public async Task<IActionResult> Index(ListPropertyModel model)
        {
            if (model == null)
                model = new ListPropertyModel();
            if (model.Filters == null)
                model.Filters = new PropertyFilters();

            PagedList<PropertyListing> properties = await _property.GetPagedProperties(model.Filters, true);
            PropertySettings settings = _site.GetPropertySettings();
            model.Properties = properties;
            model.Types = settings.GetListingTypes();
            model.PlanningTypes = settings.GetPlanningTypes();
            return View(model);
        }

        [Route("property/{id}/{city}/{postcode}/{title}")]
        public IActionResult Show(int id)
        {
            ShowPropertyModel um = new ShowPropertyModel();

            um.Property = _property.GetPropertyById(id);

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && um.Property.Status != (int)Status.Published)
                return NotFound();

            return View(um);
        }

    }

}


