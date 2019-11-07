using Hood.Core;
using Hood.Entities;
using Hood.Enums;
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
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        [Display(Name = "Latitude", Description = "This is used for mapping your property listing, you can automatically look it up with Geocoding setup correctly. Or enter it manually.")]
        public double Latitude { get; set; }
        [Display(Name = "Longitude", Description = "This is used for mapping your property listing, you can automatically look it up with Geocoding setup correctly. Or enter it manually.")]
        public double Longitude { get; set; }

        // Publish Status
        public ContentStatus Status { get; set; }

        // Dates
        [Display(Name = "Publish Date", Description = "The content will only appear on the site after this date, when set to published.")]
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
            get => FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(FeaturedImageJson) : MediaObject.Blank;
            set => FeaturedImageJson = JsonConvert.SerializeObject(value);
        }

        // Media
        public string InfoDownloadJson { get; set; }
        [NotMapped]
        public IMediaObject InfoDownload
        {
            get => InfoDownloadJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(InfoDownloadJson) : MediaObject.Blank;
            set => InfoDownloadJson = JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// The type of listing - Sale/Rental/Student/Short Term/Long Term/Sub-Lease/Commercial etc.
        /// </summary>
        [Display(Name = "Listing Type", Description = "The type of listing - Sale/Rental/Student/Short Term/Long Term/Sub-Lease/Commercial etc.")]
        public string ListingType { get; set; }

        /// <summary>
        /// Availability flag for the lease or listing - Available/Sold/Sold STC/Reserved/Let etc.
        /// </summary>
        [Display(Name = "Lease/Listing Status", Description = "Availability flag for the lease or listing - Available/Sold/Sold STC/Reserved/Let etc.")]
        public string LeaseStatus { get; set; }

        /// <summary>
        /// The property type according to RightMove. House/Terraced/Bungalow etc.
        /// </summary>
        [Display(Name = "Property Type", Description = "The property type according to RightMove specification. House/Terraced/Bungalow etc.")]
        public string PropertyType { get; set; }

        /// <summary>
        /// (Deprecated) Size of property. Currently unused.
        /// </summary> 
        public string Size { get; set; }

        /// <summary>
        /// Number of bedrooms available.
        /// </summary>
        [Display(Name = "Number of Bedrooms", Description = "Number of bedrooms in the property.")]
        public int Bedrooms { get; set; }

        /// <summary>
        /// Confidential flag, can be used to show confidentiality agreement, or restric to certain areas of the site.
        /// </summary>
        [Display(Name = "Confidential Listing", Description = "Use this option to hide confidential details about the listing, use the template to hide custom details.")]
        public bool Confidential { get; set; }

        /// <summary>
        /// Featured flag, can be used to show on featured areas of the site.
        /// </summary>
        [Display(Name = "Featured Property", Description = "This will appear in the 'featured' lists on the homepage and other areas of the site.")]
        public bool Featured { get; set; }


        /// <summary>
        /// Quick description of the property.
        /// </summary>
        [Display(Name = "Short Description", Description = "A quick description, used in listings and at the top of listing pages in default templates..")]
        public string ShortDescription { get; set; }
        /// <summary>
        /// Detailed HTML description of the property.
        /// </summary>
        [Display(Name = "Short Description", Description = "A quick description, used in listings and at the top of listing pages in default templates..")]
        public string Description { get; set; }
        /// <summary>
        /// (Optional) Additional information about the property in general. 
        /// </summary>
        [Display(Name = "Additional", Description = "Additional information about the property in general. ")]
        public string Additional { get; set; }
        /// <summary>
        /// (Optional) Additional information about the lease. 
        /// </summary>
        [Display(Name = "Lease Description", Description = "Additional information about the lease/sale.")]
        public string Lease { get; set; }
        /// <summary>
        /// (Optional) Additional information about the area. 
        /// </summary>
        [Display(Name = "Area Description", Description = "Additional information about the area.")]
        public string Areas { get; set; }
        /// <summary>
        /// (Optional) Additional information about the location. 
        /// </summary>
        [Display(Name = "Location Description", Description = "Additional information about the location.")]
        public string Location { get; set; }
        /// <summary>
        /// (Optional) Additional information about or for the agent. 
        /// </summary>
        [Display(Name = "Agent Info", Description = "Additional information about or for the agent.")]
        public string AgentInfo { get; set; }

        /// <summary>
        /// Planning Classifcation A1/A2/A3 etc.
        /// </summary>
        [Display(Name = "Planning Classifcation", Description = "Planning Classifcation A1/A2/A3 etc.")]
        public string Planning { get; set; }

        // Prices
        [Display(Name = "Rent", Description = "Numeric value for rent.")]
        public decimal? Rent { get; set; }
        [Display(Name = "Asking Price", Description = "Numeric value for asking price.")]
        public decimal? AskingPrice { get; set; }
        [Display(Name = "Premium", Description = "Numeric value for premium.")]
        public decimal? Premium { get; set; }
        [Display(Name = "Fees", Description = "Numeric value for fees.")]
        public decimal? Fees { get; set; }

        [Display(Name = "Rent Display", Description = "How the rent is displayed {0} represents the value.")]
        public string RentDisplay { get; set; }
        [Display(Name = "Asking Price Display", Description = "How the asking price is displayed {0} represents the value.")]
        public string AskingPriceDisplay { get; set; }
        [Display(Name = "Premium Display", Description = "How the premium is displayed {0} represents the value.")]
        public string PremiumDisplay { get; set; }
        [Display(Name = "Fees Display", Description = "How the fees are displayed {0} represents the value.")]
        public string FeesDisplay { get; set; }

        public string Email { get; set; }
        public string QuickName { get; set; }
        public string Addressee { get; set; }
        public string Phone { get; set; }
        public string SingleLineAddress => this.ToFormat(AddressFormat.SingleLine);

        // Agent 
        [Display(Name = "Agent/Owner", Description = "The agent or creator of this property.")]
        public string AgentId { get; set; }
        [Display(Name = "Agent/Owner", Description = "The agent or creator of this property.")]
        public ApplicationUser Agent { get; set; }

        [NotMapped]
        public List<ApplicationUser> AvailableAgents { get; set; }

        public List<PropertyMeta> Metadata { get; set; }
        public List<PropertyMedia> Media { get; set; }
        public List<PropertyFloorplan> FloorPlans { get; set; }

        public string DisplayTitle
        {
            get
            {
                if (Title.IsSet())
                {
                    return Title;
                }

                return this.ToFormat(AddressFormat.Short);
            }
        }

        [NotMapped]
        public string Url => string.Format("/property/{0}/{1}/{2}/{3}", Id, Address2.IsSet() ? Address2.ToSeoUrl() : City.ToSeoUrl(), Postcode.Split(' ').First().ToSeoUrl(), Title.ToSeoUrl());

        [NotMapped]
        public string FormattedRent
        {
            get
            {
                PropertySettings propertySettings = Engine.Settings.Property;
                decimal rent = 0;
                if (Rent.HasValue)
                {
                    rent = Rent.Value;
                }

                return propertySettings.ShowRentDecimals ? rent.ToString("C") : rent.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedAskingPrice
        {
            get
            {
                PropertySettings propertySettings = Engine.Settings.Property;
                decimal ap = 0;
                if (AskingPrice.HasValue)
                {
                    ap = AskingPrice.Value;
                }

                return propertySettings.ShowAskingPriceDecimals ? ap.ToString("C") : ap.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedPremium
        {
            get
            {
                PropertySettings propertySettings = Engine.Settings.Property;
                decimal premium = 0;
                if (Premium.HasValue)
                {
                    premium = Premium.Value;
                }

                return propertySettings.ShowPremiumDecimals ? premium.ToString("C") : premium.ToString("C0");
            }
        }

        [NotMapped]
        public string FormattedFees
        {
            get
            {
                PropertySettings propertySettings = Engine.Settings.Property;
                decimal fees = 0;
                if (Fees.HasValue)
                {
                    fees = Fees.Value;
                }

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
            get => Floors.IsSet() ? JsonConvert.DeserializeObject<List<FloorArea>>(Floors) : new List<FloorArea>();
            set => Floors = JsonConvert.SerializeObject(value);
        }

        [NotMapped]
        public decimal TotalFloorAreaMetres
        {
            get
            {
                if (FloorAreas == null)
                {
                    return 0;
                }

                if (FloorAreas.Count == 0)
                {
                    return 0;
                }

                return FloorAreas.Select(f => f.SquareMetres).Sum();
            }
        }
        [NotMapped]
        public decimal TotalFloorAreaFeet
        {
            get
            {
                if (FloorAreas == null)
                {
                    return 0;
                }

                if (FloorAreas.Count == 0)
                {
                    return 0;
                }

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
                switch (Status)
                {
                    case ContentStatus.Published:
                        if (PublishDate > DateTime.Now)
                        {
                            return "Will publish on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                        }
                        else
                        {
                            return "Published on: " + PublishDate.ToShortDateString() + " at " + PublishDate.ToShortTimeString();
                        }

                    case ContentStatus.Draft:
                    default:
                        return "Draft";
                    case ContentStatus.Archived:
                        return "Archived";
                    case ContentStatus.Deleted:
                        return "Deleted";
                }
            }
        }
        [NotMapped]
        public bool PublishPending { get; set; }
        [NotMapped]
        [Display(Name = "Auto locate address", Description = "Use the Google Address Service to lookup your address from the postcode/address information provided, and get a map Latitude/Longitude reference to use with Google Maps.")]
        public bool AutoGeocode { get; set; }
        public string QuickInfo
        {
            get
            {
                switch (ListingType)
                {
                    case "Sale":
                        return $"{Bedrooms} bedroom {PropertyType} for sale at {FormattedAskingPrice}";
                    case "Commercial":
                        return $"{PropertyType} for sale at {FormattedAskingPrice}";
                    default:
                        if (LeaseStatus == "Available")
                        {
                            return $"{Bedrooms} bedroom {PropertyType} available now for {FormattedRent}";
                        }
                        else if (LeaseStatus == "Let Agreed")
                        {
                            return $"{Bedrooms} bedroom {PropertyType}. Let Agreed.";
                        }
                        else
                        {
                            return $"{Bedrooms} bedroom {PropertyType}. Unavailable.";
                        }
                }
            }
        }

        public PropertyMeta GetMeta(string name)
        {
            PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
            if (cm == null)
            {
                return new PropertyMeta()
                {
                    BaseValue = null,
                    Name = name,
                    Type = null
                };
            }

            return cm;
        }
        public void UpdateMeta(string name, string value)
        {
            if (Metadata != null)
            {
                PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    cm.SetValue(value);
                }
            }
        }
        public void AddMeta(string name, string value, string metaType = "System.String")
        {

            var newMeta = new PropertyMeta()
            {
                Name = name,
                Type = metaType,
                PropertyId = Id
            };
            newMeta.SetValue(value);
            if (Metadata != null)
            {
                Metadata.Add(newMeta);
            }
        }
        public void RemoveMeta(string name)
        {
            if (HasMeta(name))
            {
                PropertyMeta meta = GetMeta(name);
                Metadata.Remove(meta);
            }
        }
        public bool HasMeta(string name)
        {
            if (Metadata != null)
            {
                PropertyMeta cm = Metadata.FirstOrDefault(p => p.Name == name);
                if (cm != null)
                {
                    return true;
                }
            }
            return false;
        }
        public bool BillIncludes(string type)
        {
            PropertyMeta meta = Metadata.SingleOrDefault(m => m.Name == "Bill.Includes." + type);
            if (meta == null)
            {
                return false;
            }

            bool value = meta.GetValue<bool>();
            if (value)
            {
                return true;
            }

            return false;
        }

    }

}

