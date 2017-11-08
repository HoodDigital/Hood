using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models.Api
{
    public partial class PropertyListingApi : PropertyListingApi<HoodIdentityUser>
    {
    }

    public partial class PropertyListingApi<TUser> : IAddress where TUser : IHoodUser
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Reference { get; set; }
        public string Tags { get; set; }

        // IAddress
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string QuickName { get; set; }
        public string Addressee { get; set; }
        public string Number { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string SingleLineAddress { get; set; }

        // Agent 
        public string AgentId { get; set; }

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

        // View and Sharecounts
        public int Views { get; set; }
        public int ShareCount { get; set; }

        // Featured Images
        public int? FeaturedImageId { get; set; }
        public int? BannerImageId { get; set; }

        // Media
        public int? InfoDownloadId { get; set; }

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

        public string FormattedRent { get; set; }
        public string FormattedAskingPrice { get; set; }
        public string FormattedPremium { get; set; }
        public string FormattedFees { get; set; }

        public string RentDisplay { get; set; }
        public string AskingPriceDisplay { get; set; }
        public string PremiumDisplay { get; set; }
        public string FeesDisplay { get; set; }

        // Calculated
        public List<FloorArea> FloorAreas { get; set; }
        public decimal TotalFloorAreaMetres
        {
            get
            {
                if (FloorAreas == null) return 0;
                if (FloorAreas.Count == 0) return 0;
                return FloorAreas.Select(f => f.SquareMetres).Sum();
            }
        }
        public decimal TotalFloorAreaFeet
        {
            get
            {
                if (FloorAreas == null) return 0;
                if (FloorAreas.Count == 0) return 0;
                return FloorAreas.Select(f => f.SquareFeet).Sum();
            }
        }
        public ApplicationUserApi<TUser> Agent { get; set; }
        public MediaApi FeaturedImage { get; set; }
        public MediaApi InfoDownload { get; set; }

        public List<MetaDataApi<PropertyMeta<TUser>>> Meta { get; set; }
        public List<MediaApi> Media { get; set; }
        public List<MediaApi> FloorPlans { get; set; }

        // MVVM Helpers
        public int PublishHour { get; set; }
        public int PublishMinute { get; set; }
        public string PublishDatePart { get; set; }

        // Formatted Members
        public string StatusString { get; set; }
        public bool PublishPending { get; set; }
        public string Url { get; }

        public PropertyListingApi()
        {
        }

        public PropertyListingApi(PropertyListing<TUser> post, ISettingsRepository<TUser> settings = null)
        {
            var mediaSettings = settings.GetMediaSettings();
            var propertySettings = settings.GetPropertySettings();

            if (post == null)
                return;
            post.CopyProperties(this);

            SingleLineAddress = post.ToFormat(AddressFormat.SingleLine);


            if (post.FeaturedImage != null)
                FeaturedImage = new MediaApi(post.FeaturedImage, settings);
            else
                FeaturedImage = MediaApi.Blank(mediaSettings);

            if (post.InfoDownload != null)
                InfoDownload = new MediaApi(post.InfoDownload, settings);
            else
                InfoDownload = MediaApi.Blank(mediaSettings);

            if (post.Metadata != null)
                Meta = post.Metadata.Select(c => new MetaDataApi<PropertyMeta<TUser>>(c)).ToList();
            else
                Meta = new List<MetaDataApi<PropertyMeta<TUser>>>();

            if (post.Media != null)
                Media = post.Media.Select(c => new MediaApi(c, settings)).ToList();
            else
                Media = new List<MediaApi>();

            if (post.FloorPlans != null)
                FloorPlans = post.FloorPlans.Select(c => new MediaApi(c, settings)).ToList();
            else
                FloorPlans = new List<MediaApi>();

            Agent = new ApplicationUserApi<TUser>(post.Agent, settings);

            PublishPending = false;
            PublishDatePart = PublishDate.ToShortDateString();
            PublishHour = PublishDate.Hour;
            PublishMinute = PublishDate.Minute;

            if (post.Rent.HasValue)
                FormattedRent = propertySettings.ShowRentDecimals ? post.Rent.Value.ToString("C") : post.Rent.Value.ToString("C0");
            if (post.AskingPrice.HasValue)
                FormattedAskingPrice = propertySettings.ShowAskingPriceDecimals ? post.AskingPrice.Value.ToString("C") : post.AskingPrice.Value.ToString("C0");
            if (post.Premium.HasValue)
                FormattedPremium = propertySettings.ShowPremiumDecimals ? post.Premium.Value.ToString("C") : post.Premium.Value.ToString("C0");
            if (post.Fees.HasValue)
                FormattedFees = propertySettings.ShowFeesDecimals ? post.Fees.Value.ToString("C") : post.Fees.Value.ToString("C0");



            if (Status == 1)
            {
                StatusString = "Draft <span>(Provisional publish date " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString() + ")</span>";
            }
            else if (Status == 3)
            {
                StatusString = "Archived";
            }
            else
            {
                if (PublishDate > DateTime.Now)
                {
                    PublishPending = true;
                    StatusString = "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                }
                else
                {
                    StatusString = "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                }
            }

            try
            {
                FloorAreas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FloorArea>>(Floors);
            }
            catch (Exception)
            {
                Floors = "";
                FloorAreas = new List<FloorArea>();
            }
        }

        public bool BillIncludes(string type)
        {
            var meta = Meta.SingleOrDefault(m => m.Name == "Bill.Includes." + type);
            if (meta == null)
                return false;
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<bool>(meta.Value);
            if (value)
                return true;
            return false;
        }

    }

}
