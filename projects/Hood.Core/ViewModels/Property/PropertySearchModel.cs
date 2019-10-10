using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class PropertyListModel : PagedList<PropertyListing>, IPageableModel
    {
        public PropertyListModel() : base()
        {
            PageSize = Engine.Settings.Property.DefaultPageSize;
            if (PageSize <= 0)
                PageSize = 20;
            PageIndex = 1;

            PublishStatus = ContentStatus.Published;
        }
        /// <summary>
        /// Specify the exact filter term for listing type - cannot be used in conjunction with Transaction, which specifies generally. 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Generalised filter term for Sale/Student/Rental - cannot be used in conjunction with Type, which specifies exactly.
        /// </summary>
        public string Transaction { get; set; }

        /// <summary>
        /// Filter by planning type.
        /// </summary>
        public string PlanningType { get; set; }
        public string Location { get; set; }
        /// <summary>
        /// Set this as -1 in order to use MaxBedrooms as a high filter.  
        /// </summary>
        public int? Bedrooms { get; set; }
        /// <summary>
        /// Set this as -1 in order to use Bedrooms as a low filter.  
        /// </summary>
        public int? MaxBedrooms { get; set; }
        public int? MaxRent { get; set; }
        public int? MinRent { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPremium { get; set; }
        public int? MinPremium { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Agent { get; set; }

        /// <summary>
        /// Filter by availability for the lease or listing - Available/Sold/Sold STC/Reserved/Let etc.
        /// </summary>
        public string Status { get; set; }

        public List<string> Types { get; set; }
        public Dictionary<string, string> PlanningTypes { get; set; }
        public List<MapMarker> Locations { get; set; }
        public GeoCoordinate CentrePoint { get; set; }
        public ContentStatus? PublishStatus { get; set; }
        public bool Featured { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Type.IsSet() ? "&type=" + Type : "";
            query += PlanningType.IsSet() ? "&planning=" + PlanningType : "";
            query += Bedrooms.HasValue ? "&Bedrooms=" + Bedrooms : "";
            query += MinPremium.HasValue ? "&MinPremium=" + MinPremium : "";
            query += MaxPremium.HasValue ? "&MaxPremium=" + MaxPremium : "";
            query += MinPrice.HasValue ? "&MinPrice=" + MinPrice : "";
            query += MaxPrice.HasValue ? "&MaxPrice=" + MaxPrice : "";
            query += MinRent.HasValue ? "&MinRent=" + MinRent : "";
            query += MaxRent.HasValue ? "&MaxRent=" + MaxRent : "";
            query += PublishStatus.HasValue ? "&publishStatus=" + PublishStatus : "";
            return query;
        }
    }
}