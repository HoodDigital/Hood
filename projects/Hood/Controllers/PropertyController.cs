using Hood.Enums;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class PropertyController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {

        public PropertyController()
            : base()
        { }

        public async Task<IActionResult> Index(PropertySearchModel model)
        {
            var propertySettings = _settings.GetPropertySettings();
            if (!propertySettings.Enabled || !propertySettings.ShowList)
                return NotFound();

            model = await _property.GetPagedProperties(model, true);

            model.Locations = await _property.GetLocations(model);
            model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            PropertySettings settings = _settings.GetPropertySettings();
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

            if (um.Property == null)
                return RedirectToAction("NotFound");

            // if not admin, and not published, hide.
            if (!(User.IsInRole("Admin") || User.IsInRole("Editor")) && um.Property.Status != (int)Status.Published)
                return RedirectToAction("NotFound");

            return View(um);
        }

        [Route("property/not-found")]
        public IActionResult NotFound(int id)
        {
            return View();
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


