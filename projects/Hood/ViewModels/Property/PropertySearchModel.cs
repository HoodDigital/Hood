using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class PropertySearchModel : PagedList<PropertyListing>, IPageableModel
    {
        public PropertySearchModel()
        {
            var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
            var propertySettings = siteSettings.GetPropertySettings();
            PageSize = propertySettings.DefaultPageSize;
            if (PageSize <= 0)
                PageSize = 20;
            PageIndex = 1;
        }

        public string Order { get; set; }
        public string Search { get; set; }

        public string Type { get; set; }
        public string Transaction { get; set; }
        public string PlanningType { get; set; }
        public string Location { get; set; }
        public int? Bedrooms { get; set; }
        public int? MaxRent { get; set; }
        public int? MinRent { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPremium { get; set; }
        public int? MinPremium { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Agent { get; set; }
        public string Status { get; set; }

        public List<string> Types { get; set; }
        public Dictionary<string, string> PlanningTypes { get; set; }
        public List<MapMarker> Locations { get; set; }
        public GeoCoordinate CentrePoint { get; internal set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Type.IsSet() ? "&type=" + Type : "";
            query += PlanningType.IsSet() ? "&planning=" + PlanningType : "";
            query += Bedrooms.HasValue ? "&Bedrooms=" + Bedrooms : "";
            query += MinPremium.HasValue ? "&MinPremium=" + MinPremium : "";
            query += MaxPremium.HasValue ? "&MaxPremium=" + MaxPremium : "";
            query += MinPrice.HasValue ? "&MinPrice=" + MinPrice : "";
            query += MaxPrice.HasValue ? "&MaxPrice=" + MaxPrice : "";
            query += MinRent.HasValue ? "&MinRent=" + MinRent : "";
            query += MaxRent.HasValue ? "&MaxRent=" + MaxRent : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }
    }
}