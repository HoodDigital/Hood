using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class

    public partial class PropertyListing : PropertyListingBase
    {
        // Agent 
        public string AgentId { get; set; }
        public HoodIdentityUser Agent { get; set; }

        public List<PropertyMeta> Metadata { get; set; }
        public List<PropertyMedia> Media { get; set; }
        public List<PropertyFloorplan> FloorPlans { get; set; }
    }
    public partial class PropertyListing<TUser> : PropertyListingBase where TUser : IHoodUser
    {
        // Agent 
        public string AgentId { get; set; }
        public TUser Agent { get; set; }

        public List<PropertyMeta<TUser>> Metadata { get; set; }
        public List<PropertyMedia<TUser>> Media { get; set; }
        public List<PropertyFloorplan<TUser>> FloorPlans { get; set; }
    }
    public partial class PropertyListingBase : BaseEntity, IAddress
    {

        // Content
        public string Title { get; set; }
        public string Reference { get; set; }
        public string Tags { get; set; }

        // IAddress
        public string ContactName { get; set; }
        [Display(Name = "Building Name/Number")]
        public string Number { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Publish Status
        public int Status { get; set; }

        // Dates
        public DateTime PublishDate { get; set; }

        // Creator/Editor
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }


        // Logs and notes
        public string UserVars { get; set; }
        public string Notes { get; set; }
        public string SystemNotes { get; set; }

        // Content Setting
        public bool AllowComments { get; set; }
        public bool Public { get; set; }
        public int Views { get; set; }
        public int ShareCount { get; set; }

        // Featured Images
        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public SiteMedia FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<SiteMedia>(FeaturedImageJson) : null; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        // Media
        public string InfoDownloadJson { get; set; }
        [NotMapped]
        public SiteMedia InfoDownload
        {
            get { return InfoDownloadJson.IsSet() ? JsonConvert.DeserializeObject<SiteMedia>(InfoDownloadJson) : null; }
            set { InfoDownloadJson = JsonConvert.SerializeObject(value); }
        }

        // Settings
        public string ListingType { get; set; }
        public string LeaseStatus { get; set; }
        public string PropertyType { get; set; }
        public string Size { get; set; }
        public int Bedrooms { get; set; }
        public string Floors { get; set; }
        public bool Confidential { get; set; }
        public bool Featured { get; set; }

        // Descriptions
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Additional { get; set; }
        public string Lease { get; set; }
        public string Areas { get; set; }
        public string Location { get; set; }
        public string AgentInfo { get; set; }

        // Planning
        public string Planning { get; set; }

        // Prices
        public decimal? Rent { get; set; }
        public decimal? AskingPrice { get; set; }
        public decimal? Premium { get; set; }
        public decimal? Fees { get; set; }

        public string RentDisplay { get; set; }
        public string AskingPriceDisplay { get; set; }
        public string PremiumDisplay { get; set; }
        public string FeesDisplay { get; set; }

        [NotMapped]
        public string Url
        {
            get
            {
                return string.Format("/property/{0}/{1}/{2}/{3}", Id, Address2.IsSet() ? Address2.ToSeoUrl() : City.ToSeoUrl(), Postcode.Split(' ').First().ToSeoUrl(), Title.ToSeoUrl());

            }
        }
    }

}

