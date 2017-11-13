using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.Linq;
using Hood.Enums;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Controllers
{
    //[Area("Hood")]
    public class PropertyController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _property;
        private readonly ISettingsRepository _settings;
        private readonly IHostingEnvironment _env;
        private readonly IBillingService _billing;

        public PropertyController(
            IPropertyRepository property,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository site,
            IBillingService billing,
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _property = property;
            _settings = site;
            _billing = billing;
            _env = env;
        }

        public async Task<IActionResult> Index(ListPropertyModel model)
        {
            var propertySettings = _settings.GetPropertySettings();
            if (!propertySettings.Enabled || !propertySettings.ShowList)
                return NotFound();

            if (model == null)
                model = new ListPropertyModel();
            if (model.Filters == null)
                model.Filters = new PropertyFilters();

            PagedList<PropertyListing> properties = await _property.GetPagedProperties(model.Filters, true);
            model.Locations = await _property.GetLocations(model.Filters);
            model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            PropertySettings settings = _settings.GetPropertySettings();
            model.Properties = properties;
            model.Types = settings.GetListingTypes();
            model.PlanningTypes = settings.GetPlanningTypes();
            return View(model);
        }

        [Route("property/{id}/{city}/{postcode}/{title}")]
        public IActionResult Show(int id)
        {
            var propertySettings = _settings.GetPropertySettings();
            if (!propertySettings.Enabled || !propertySettings.ShowItem)
                return NotFound();

            ShowPropertyModel um = new ShowPropertyModel()
            {
                Property = _property.GetPropertyById(id)
            };

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && um.Property.Status != (int)Status.Published)
                return NotFound();

            return View(um);
        }

        [Route("property/modal")]
        public IActionResult Modal(int id)
        {
            ShowPropertyModel um = new ShowPropertyModel()
            {
                Property = _property.GetPropertyById(id)
            };

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && um.Property.Status != (int)Status.Published)
                return NotFound();

            return View(um);
        }
    }

}


