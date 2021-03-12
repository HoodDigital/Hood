using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
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

            Type = new List<string>();
            Status = new List<string>();
        }

        /// <summary>
        /// Specify the exact filter term for listing type - cannot be used in conjunction with Transaction, which specifies generally. 
        /// </summary>
        [FromQuery(Name = "type")]
        public List<string> Type { get; set; } = new List<string>();


        /// <summary>
        /// Filter by availability for the lease or listing - Available/Sold/Sold STC/Reserved/Let etc.
        /// </summary>
        [FromQuery(Name = "status")]
        public List<string> Status { get; set; }

        /// <summary>
        /// for internal use only, forces loading of images in the search, not recommended.
        /// </summary>
        [FromQuery(Name = "img")]
        public bool LoadImages { get; set; } = false;

        /// <summary>
        /// Filter by planning type.
        /// </summary>
        [FromQuery(Name = "planning")]
        public string PlanningType { get; set; }

        [FromQuery(Name = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Set this as -1 in order to use MaxBedrooms as a high filter.  
        /// </summary>
        [FromQuery(Name = "beds")]
        public int? Bedrooms { get; set; }

        /// <summary>
        /// Set this as -1 in order to use Bedrooms as a low filter.  
        /// </summary>
        [FromQuery(Name = "beds-min")]
        public int? MinBedrooms { get; set; }
        /// <summary>
        /// Set this as -1 in order to use Bedrooms as a low filter.  
        /// </summary>
        [FromQuery(Name = "beds-max")]
        public int? MaxBedrooms { get; set; }
        [FromQuery(Name = "rent-min")]
        public int? MaxRent { get; set; }
        [FromQuery(Name = "rent-max")]
        public int? MinRent { get; set; }
        [FromQuery(Name = "price-min")]
        public int? MaxPrice { get; set; }
        [FromQuery(Name = "price-max")]
        public int? MinPrice { get; set; }
        [FromQuery(Name = "prem-min")]
        public int? MaxPremium { get; set; }
        [FromQuery(Name = "prem-max")]
        public int? MinPremium { get; set; }
        [FromQuery(Name = "agent")]
        public string Agent { get; set; }

        [FromQuery(Name = "featured")]
        public bool Featured { get; set; }

        #region View Model Stuff

        /// <summary>
        /// Used to store the listings types available for the selection
        /// </summary>
        public List<string> AvailableTypes { get; set; }
        public List<string> AvailableStatuses { get; set; }
        public List<string> AvailablePlanningTypes { get; set; }
        public Dictionary<string, string> PlanningTypes { get; set; }
        public List<MapMarker> Locations { get; set; }
        public GeoCoordinate CentrePoint { get; set; }
        public ContentStatus? PublishStatus { get; set; }

        #endregion

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            foreach (var type in Type)
            {
                query += "&type=" + type;
            }
            foreach (var status in Status)
            {
                query += "&status=" + status;
            }
            query += PlanningType.IsSet() ? "&planning=" + PlanningType : "";
            query += LoadImages ? "&img=true" : "";
            query += Featured ? "&featured=true" : "";
            query += Location.IsSet() ? "&location=" + Location : "";
            query += Agent.IsSet() ? "&agent=" + Agent : "";
            query += PlanningType.IsSet() ? "&planning=" + PlanningType : "";
            query += Bedrooms.HasValue ? "&beds=" + Bedrooms : "";
            query += MinBedrooms.HasValue ? "&beds-min=" + MinBedrooms : "";
            query += MaxBedrooms.HasValue ? "&beds-max=" + MaxBedrooms : "";
            query += MinPremium.HasValue ? "&prem-min=" + MinPremium : "";
            query += MaxPremium.HasValue ? "&prem-max=" + MaxPremium : "";
            query += MinPrice.HasValue ? "&price-min=" + MinPrice : "";
            query += MaxPrice.HasValue ? "&price-max=" + MaxPrice : "";
            query += MinRent.HasValue ? "&rent-min=" + MinRent : "";
            query += MaxRent.HasValue ? "&rent-max=" + MaxRent : "";
            query += PublishStatus.HasValue ? "&publish=" + PublishStatus : "";
            return query;
        }
    }
}