using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Web.Controllers
{
    public class PropertyController : Hood.Controllers.PropertyController
    {
        public PropertyController()
            : base()
        { }

        [Route("/properties/favourites")]
        public async Task<IActionResult> Favourites(PropertyListModel model)
        {
            model.Type = new List<string>();
            return await FavouritesList(model, new List<int>(), nameof(Favourites));
        }

        [Route("/properties/favourites/share")]
        public async Task<IActionResult> ShareFavourites(PropertyListModel model, List<int> f = null)
        {
            model.Type = new List<string>();
            return await FavouritesList(model, f, nameof(ShareFavourites));
        }

        [Route("/properties/favourites/list")]
        public async Task<IActionResult> FavouritesList(PropertyListModel model, List<int> f = null, string viewName = "_List_Favourites")
        {
            IQueryable<PropertyListing> properties = _db.Properties
                 .Include(p => p.Agent)
                 .Include(p => p.Metadata)
                 .Include(p => p.Media)
                 .Where(p => p.Status == model.PublishStatus);

            if (model.Search.IsSet())
            {
                properties = properties.Where(n =>
                    n.Title.Contains(model.Search) ||
                    n.Address1.Contains(model.Search) ||
                    n.Address2.Contains(model.Search) ||
                    n.City.Contains(model.Search) ||
                    n.County.Contains(model.Search) ||
                    n.Postcode.Contains(model.Search) ||
                    n.ShortDescription.Contains(model.Search) ||
                    n.Lease.Contains(model.Search) ||
                    n.Location.Contains(model.Search) ||
                    n.Planning.Contains(model.Search) ||
                    n.Reference.Contains(model.Search)
                );
            }

            properties = properties.Where(p => f.Contains(p.Id));

            model.AvailableTypes = await _db.Properties.Select(p => p.ListingType).Distinct().ToListAsync();
            model.AvailableStatuses = await _db.Properties.Select(p => p.LeaseStatus).Distinct().ToListAsync();
            model.AvailablePlanningTypes = await _db.Properties.Select(p => p.Planning).Distinct().ToListAsync();
            model.PlanningTypes = Engine.Settings.Property.GetPlanningTypes();

           await model.ReloadAsync(properties);

            return View(viewName, model);
        }


        [Route("/property")]
        public IActionResult RedirectLegacy()
        {
            return RedirectToActionPermanent(nameof(Index));
        }

        [Route("/properties")]
        public override async Task<IActionResult> Index(PropertyListModel model)
        {
            if (model.Type.Count == 0)
            {
                return RedirectToActionPermanent(nameof(Index), new { type = "Student" });
            }
            return await List(model, nameof(Index));
        }

        [Route("/properties/list")]
        public override async Task<IActionResult> List(PropertyListModel model, string viewName = "_List_Properties")
        {
            model.LoadImages = true;
            model = await _property.GetPropertiesAsync(model);
            return View(viewName, model);
        }

        [Route("/properties/map")]
        public override async Task<IActionResult> Map(PropertyListModel model)
        {
            var locations = await _property.GetLocationsAsync(model);
            return View("_Map_Properties", locations);
        }

        [Route("property/{id:int}/{city?}/{postcode?}/{title?}")]
        public override async Task<IActionResult> Show(int id)
        {
            return await base.Show(id);
        }

        [Route("property/modal")]
        public override async Task<IActionResult> Modal(int id)
        {
            return await base.Modal(id);
        }

        public PropertyListModel GetProperties(PropertyListModel model)
        {
            return model;
        }
    }
}
