using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public partial class PropertyListing : BaseEntity, IAddress, IMetaObect<PropertyMeta>
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
        public IMediaObject FeaturedImage
        {
            get { return FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(FeaturedImageJson) : MediaObject.Blank; }
            set { FeaturedImageJson = JsonConvert.SerializeObject(value); }
        }

        // Media
        public string InfoDownloadJson { get; set; }
        [NotMapped]
        public IMediaObject InfoDownload
        {
            get { return InfoDownloadJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(InfoDownloadJson) : MediaObject.Blank; }
            set { InfoDownloadJson = JsonConvert.SerializeObject(value); }
        }

        /// <summary>
        /// The type of listing - Sale/Rental/Student/Short Term/Long Term/Sub-Lease/Commercial etc.
        /// </summary>
        public string ListingType { get; set; }

        /// <summary>
        /// Availability flag for the lease or listing - Available/Sold/Sold STC/Reserved/Let etc.
        /// </summary>
        public string LeaseStatus { get; set; }

        /// <summary>
        /// The property type according to RightMove. House/Terraced/Bungalow etc.
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// (Deprecated) Size of property. Currently unused.
        /// </summary> 
        public string Size { get; set; }

        /// <summary>
        /// Number of bedrooms available.
        /// </summary>
        public int Bedrooms { get; set; }

        /// <summary>
        /// Confidential flag, can be used to show confidentiality agreement, or restric to certain areas of the site.
        /// </summary>
        public bool Confidential { get; set; }

        /// <summary>
        /// Featured flag, can be used to show on featured areas of the site.
        /// </summary>
        public bool Featured { get; set; }


        /// <summary>
        /// Quick description of the property.
        /// </summary>
        public string ShortDescription { get; set; }
        /// <summary>
        /// Detailed HTML description of the property.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// (Optional) Additional information about the property in general. 
        /// </summary>
        public string Additional { get; set; }
        /// <summary>
        /// (Optional) Additional information about the lease. 
        /// </summary>
        public string Lease { get; set; }
        /// <summary>
        /// (Optional) Additional information about the area. 
        /// </summary>
        public string Areas { get; set; }
        /// <summary>
        /// (Optional) Additional information about the location. 
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// (Optional) Additional information about or for the agent. 
        /// </summary>
        public string AgentInfo { get; set; }

       /// <summary>
       /// Planning Classifcation A1/A2/A3 etc.
       /// </summary>
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

        public string Email { get; set; }
        public string QuickName { get; set; }
        public string Addressee { get; set; }
        public string Phone { get; set; }
        public string SingleLineAddress => this.ToFormat(AddressFormat.SingleLine);

        // Agent 
        public string AgentId { get; set; }
        public ApplicationUser Agent { get; set; }

        [NotMapped]
        public List<ApplicationUser> AvailableAgents { get; set; }

        public List<PropertyMeta> Metadata { get; set; }
        public List<PropertyMedia> Media { get; set; }
        public List<PropertyFloorplan> FloorPlans { get; set; }

        [NotMapped]
        public string Url
        {
            get
            {
                return string.Format("/property/{0}/{1}/{2}/{3}", Id, Address2.IsSet() ? Address2.ToSeoUrl() : City.ToSeoUrl(), Postcode.Split(' ').First().ToSeoUrl(), Title.ToSeoUrl());
            }
        }

        [NotMapped]
        public string FormattedRent
        {
            get
            {
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                PropertySettings propertySettings = siteSettings.GetPropertySettings();
                decimal rent = 0;
                if (Rent.HasValue)
                    rent = Rent.Value;
                return propertySettings.ShowRentDecimals ? rent.ToString("C") : rent.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedAskingPrice
        {
            get
            {
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                PropertySettings propertySettings = siteSettings.GetPropertySettings();
                decimal ap = 0;
                if (AskingPrice.HasValue)
                    ap = AskingPrice.Value;
                return propertySettings.ShowAskingPriceDecimals ? ap.ToString("C") : ap.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedPremium
        {
            get
            {
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                PropertySettings propertySettings = siteSettings.GetPropertySettings();
                decimal premium = 0;
                if (Premium.HasValue)
                    premium = Premium.Value;
                return propertySettings.ShowPremiumDecimals ? premium.ToString("C") : premium.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedFees
        {
            get
            {
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                PropertySettings propertySettings = siteSettings.GetPropertySettings();
                decimal fees = 0;
                if (Fees.HasValue)
                    fees = Fees.Value;
                return propertySettings.ShowFeesDecimals ? fees.ToString("C") : fees.ToString("C0");
            }
        }


        /// <summary>
        /// Restricted field, used for JSON floor areas.
        /// </summary>
        public string Floors { get; set; }

        [NotMapped]
        public List<FloorArea> FloorAreas
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<FloorArea>>(Floors);
                }
                catch (Exception)
                {
                    return new List<FloorArea>();
                }
            }
        }
        [NotMapped]
        public decimal TotalFloorAreaMetres
        {
            get
            {
                if (FloorAreas == null) return 0;
                if (FloorAreas.Count == 0) return 0;
                return FloorAreas.Select(f => f.SquareMetres).Sum();
            }
        }
        [NotMapped]
        public decimal TotalFloorAreaFeet
        {
            get
            {
                if (FloorAreas == null) return 0;
                if (FloorAreas.Count == 0) return 0;
                return FloorAreas.Select(f => f.SquareFeet).Sum();
            }
        }

        // MVVM Helpers
        [NotMapped]
        public int PublishHour => PublishDate.Hour;
        [NotMapped]
        public int PublishMinute => PublishDate.Minute;
        [NotMapped]
        public string PublishDatePart => PublishDate.ToShortDateString();

        // Formatted Members
        [NotMapped]
        public string StatusString
        {
            get
            {
                switch ((Enums.Status)Status)
                {
                    case Enums.Status.Published:
                        if (PublishDate > DateTime.Now)
                            return "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                        else
                            return "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                    case Enums.Status.Draft:
                    default:
                        return "Draft";
                    case Enums.Status.Archived:
                        return "Archived";
                    case Enums.Status.Deleted:
                        return "Deleted";
                }
            }
        }
        [NotMapped]
        public bool PublishPending { get; set; }
        [NotMapped]
        public bool AutoGeocode { get; set; }

        public PropertyMeta GetMeta(string name)
        {
            PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
            if (cm == null)
                return new PropertyMeta()
                {
                    BaseValue = null,
                    Name = name,
                    Type = null
                };
            return cm;
        }
        public void UpdateMeta<T>(string name, T value)
        {
            if (Metadata != null)
            {
                PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    cm.Set(value);
                }
            }
        }
        public void AddMeta(PropertyMeta value)
        {
            if (Metadata != null)
            {
                Metadata.Add(value);
            }
        }
        public bool HasMeta(string name)
        {
            if (Metadata != null)
            {
                PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                    return true;
            }
            return false;
        }
        public bool BillIncludes(string type)
        {
            var meta = Metadata.SingleOrDefault(m => m.Name == "Bill.Includes." + type);
            if (meta == null)
                return false;
            var value = meta.GetValue<bool>();
            if (value)
                return true;
            return false;
        }

    }

}

