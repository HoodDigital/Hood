using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    public class PropertyController : BaseController
    {
        public PropertyController()
            : base()
        { }

        public async Task<IActionResult> Index(PropertyListModel model)
        {
            var propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowList)
                return NotFound();

            model.PublishStatus = ContentStatus.Published;
            model = await _property.GetPropertiesAsync(model);

            model.Locations = await _property.GetLocationsAsync(model);
            model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            PropertySettings settings = Engine.Settings.Property;
            model.Types = settings.GetListingTypes();
            model.PlanningTypes = settings.GetPlanningTypes();

            return View(model);
        }

        [Route("property/{id}/{city}/{postcode}/{title}")]
        public async Task<IActionResult> Show(int id)
        {
            var propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowItem)
                return NotFound();

            ShowPropertyModel um = new ShowPropertyModel()
            {
                Property = await _property.GetPropertyByIdAsync(id)
            };

            if (um.Property == null)
                return RedirectToAction("NotFound");

            // if not admin, and not published, hide.
            if (!(User.IsEditorOrBetter()) && um.Property.Status != ContentStatus.Published)
                return RedirectToAction("NotFound");

            return View(um);
        }

        [Route("property/modal")]
        public async Task<IActionResult> Modal(int id)
        {
            ShowPropertyModel um = new ShowPropertyModel()
            {
                Property = await _property.GetPropertyByIdAsync(id)
            };

            // if not admin, and not published, hide.
            if (!User.IsEditorOrBetter() && um.Property.Status != ContentStatus.Published)
                return NotFound();

            return View(um);
        }
    }

}


