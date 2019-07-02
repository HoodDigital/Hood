using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
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
            var propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowList)
                return NotFound();

            model = await _property.GetPagedProperties(model, true);

            model.Locations = await _property.GetLocations(model);
            model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            PropertySettings settings = Engine.Settings.Property;
            model.Types = settings.GetListingTypes();
            model.PlanningTypes = settings.GetPlanningTypes();

            return View(model);
        }

        [Route("property/{id}/{city}/{postcode}/{title}")]
        public IActionResult Show(int id)
        {
            var propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowItem)
                return NotFound();

            ShowPropertyModel um = new ShowPropertyModel()
            {
                Property = _property.GetPropertyById(id)
            };

            if (um.Property == null)
                return RedirectToAction("NotFound");

            // if not admin, and not published, hide.
            if (!(User.IsEditorOrBetter()) && um.Property.Status != (int)Status.Published)
                return RedirectToAction("NotFound");

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
            if (!User.IsEditorOrBetter() && um.Property.Status != (int)Status.Published)
                return NotFound();

            return View(um);
        }
    }

}


