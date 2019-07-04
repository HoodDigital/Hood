using Hood.BaseTypes;
using Hood.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hood.Models
{
    public class PropertySettings : SaveableModel
    {
        [Display(Name = "Url Slug", Description = "<strong class=\"text-danger\"><i class='fa fa-exclamation-triangle'></i> Changing this will affect external linking.</strong><br />Use this to change the default URL for your property listings. Default is <code>property</code>. Only use lowercase letters, no spaces.")]
        public string Slug { get; set; }
        [Display(Name = "Property Listings Name", Description = "Title of your property listing area, e.g. \"Property\" or \"Listing\"")]
        public string Name { get; set; }
        [Display(Name = "Enable Properties", Description = "Enable the property listing display and management tool on this website.")]
        public bool Enabled { get; set; }
        [Display(Name = "Show List Page", Description = "Show the listing page, accessible via <code>yourdomain.com/{property-url-slug}</code>")]
        public bool ShowList { get; set; }
        [Display(Name = "Show List Page", Description = "Show the item page, accessible via <code>yourdomain.com/{property-url-slug}/{id}/{address}</code>")]
        public bool ShowItem { get; set; }
        [Display(Name = "No Image (Override)", Description = "Override the site default 'No Image' for properties. Leave it blank to use the site default.")]
        public string NoImage { get; set; }
        [Display(Name = "Property Listings Title", Description = "Title of your property listing area, e.g. \"Properties\" or \"Our Listings\"")]
        public string Title { get; set; }
        [Display(Name = "Property Listings Name (Plural)", Description = "Name of your property listing, e.g. \"Properties\" or \"Listings\"")]
        public string Plural { get; set; }
        [Display(Name = "Listing Types", Description = "Options for types of property listings on the site, Commercial, Student, Short-Term, Sale etc.")]
        public string ListingTypes { get; set; }
        [Display(Name = "Asking Price Displays", Description = "Options for displaying your asking prices, the {0} signifies the amount value formatted in your currency.")]
        public string PriceDisplays { get; set; }
        [Display(Name = "Rent Displays", Description = "Options for displaying your rent, the {0} signifies the amount value formatted in your currency.<br />")]
        public string RentDisplays { get; set; }
        [Display(Name = "Property Type", Description = "Options for property types for the property listings on the site, House, Flat, Land etc.<br />")]
        public string PropertyType { get; set; }
        [Display(Name = "Planning Type", Description = "Options for planning classes for the property listings on the site.<br />")]
        public string PlanningType { get; set; }
        [Display(Name = "Fees Display", Description = "Options for displaying your fees, the {0} signifies the amount value formatted in your currency.<br />")]
        public string FeesDisplay { get; set; }
        [Display(Name = "Lease Statuses", Description = "Options for  statuses for the property listings on the site.<br />")]
        public string LeaseStatuses { get; set; }
        [Display(Name = "Trigger Authorisation Key", Description = "<span class=\"text-danger\"><i class='fa fa-exclamation-triangle'></i> Do not reveal this key to anyone or publish it anywhere.</span><br />This is used when triggering the importer using a scheduled service. Send a POST request to <code>yourdomain.com/admin/property/import/blm/trigger</code> and set this key value as the \"Auth\" header.")]
        public string TriggerAuthKey { get; set; }
        [Display(Name = "Show Bedrooms", Description = "Show number of bedrooms on property listings.")]
        public bool ShowBedrooms { get; set; }
        [Display(Name = "Show Bathrooms", Description = "Show number of batrhooms on property listings.")]
        public bool ShowBathrooms { get; set; }
        [Display(Name = "Show Showers", Description = "Show number of showers on property listings.")]
        public bool ShowShowers { get; set; }
        [Display(Name = "Show Rent", Description = "Show rent on property listings.")]
        public bool ShowRent { get; set; }
        [Display(Name = "Show Rent Decimals", Description = "Show decimal numbers for rent.")]
        public bool ShowRentDecimals { get; set; }
        [Display(Name = "Rent Minimum", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int RentMinimum { get; set; }
        [Display(Name = "Rent Maximum", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int RentMaximum { get; set; }
        [Display(Name = "Rent Increment", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int RentIncrement { get; set; }
        [Display(Name = "Show Asking Price", Description = "Show an asking price or price on sale property listings..")]
        public bool ShowAskingPrice { get; set; }
        [Display(Name = "Show Asking Price Decimals", Description = "Show decimal numbers for asking prices.")]
        public bool ShowAskingPriceDecimals { get; set; }
        [Display(Name = "Asking Price Minimum", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int AskingPriceMinimum { get; set; }
        [Display(Name = "Asking Price Maximum", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int AskingPriceMaximum { get; set; }
        [Display(Name = "Asking Price Increment", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int AskingPriceIncrement { get; set; }
        [Display(Name = "Show Premium", Description = "Display premiums on property listings.")]
        public bool ShowPremium { get; set; }
        [Display(Name = "Show Premium Decimals", Description = "Show decimal numbers for premiums.")]
        public bool ShowPremiumDecimals { get; set; }
        [Display(Name = "Premium Minimum", Description = "Minimum on searching dropdowns/sliders for premiums.")]
        public int PremiumMinimum { get; set; }
        [Display(Name = "Premium Maximum", Description = "Maximum on searching dropdowns/sliders for premiums.")]
        public int PremiumMaximum { get; set; }
        [Display(Name = "Premium Increment", Description = "Increment on searching dropdowns/sliders for premiums.")]
        public int PremiumIncrement { get; set; }
        [Display(Name = "Show Fees", Description = "Show fees on property listings.")]
        public bool ShowFees { get; set; }
        [Display(Name = "Show Fees Decimals", Description = "Show decimal numbers for fees.")]
        public bool ShowFeesDecimals { get; set; }
        [Display(Name = "Default Page Size", Description = "Number of properties displayed per page on the listing pages.")]
        public int DefaultPageSize { get; set; }

        [Display(Name = "FTP Importer Settings", Description = "")]
        public PropertyImporterSettings FTPImporterSettings { get; set; }

        public bool IsBlmImporterEnabled
        {
            get
            {
                if (FTPImporterSettings == null) return false;

                if (!FTPImporterSettings.Enabled) return false;

                switch (FTPImporterSettings.Method)
                {
                    case PropertyImporterMethod.Directory:
                        if (!FTPImporterSettings.LocalFolder.IsSet() ||
                            !FTPImporterSettings.Filename.IsSet())
                            return false;
                        break;
                    case PropertyImporterMethod.FtpBlm:
                        if (!FTPImporterSettings.Server.IsSet() ||
                            !FTPImporterSettings.Password.IsSet() ||
                            !FTPImporterSettings.Filename.IsSet())
                            return false;
                        break;
                }


                if (FTPImporterSettings.RequireUnzip)
                {
                    if (!FTPImporterSettings.ZipFile.IsSet())
                        return false;
                }
                return true;
            }
        }

        public PropertySettings()
        {
            Name = "Property";
            Plural = "Properties";
            Title = "Property";
            Slug = "property";
            ShowRent = true;
            ShowAskingPrice = true;
            ShowItem = true;
            ShowList = true;
            DefaultPageSize = 24;
            RentMinimum = 0;
            RentMaximum = 1000000;
            AskingPriceMinimum = 0;
            AskingPriceMaximum = 10000000;
            PremiumMinimum = 0;
            PremiumMaximum = 1000000;
            RentIncrement = 10000;
            AskingPriceIncrement = 50000;
            PremiumIncrement = 10000;
            Enabled = false;
            PriceDisplays = PropertyDetails.PriceQualifiers.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            RentDisplays = PropertyDetails.RentFrequency.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            FeesDisplay = PropertyDetails.Fees.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            PropertyType = PropertyDetails.PropertyTypes.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            PlanningType = PropertyDetails.PlanningTypes.OrderBy(c => c.Key).Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(":", a.Key, a.Value)), sb => sb.ToString());
            ListingTypes = PropertyDetails.ListingTypes.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
            LeaseStatuses = PropertyDetails.LeaseStatuses.OrderBy(c => c.Value).Select(p => p.Value).Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(a), sb => sb.ToString());
        }

        public Dictionary<string, string> GetPlanningTypes()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (string s in Regex.Split(PlanningType, "\r\n|\r|\n"))
            {
                string[] ss = s.Split(':');
                if (ss.Length > 1)
                {
                    var key = s.Split(':')[0];
                    var itm = s.Split(':')[1];
                    if (key.Trim().IsSet() && itm.Trim().IsSet())
                        ret.Add(key, itm);
                }
            }
            return ret;
        }

        public List<string> GetListingTypes()
        {
            List<string> ret = new List<string>();
            foreach (string s in Regex.Split(ListingTypes, "\r\n|\r|\n"))
            {
                if (s.Trim().IsSet())
                    ret.Add(s);
            }
            return ret;
        }

        public List<string> GetLeaseStatuses()
        {
            List<string> ret = new List<string>();
            foreach (string s in Regex.Split(LeaseStatuses, "\r\n|\r|\n"))
            {
                if (s.Trim().IsSet())
                    ret.Add(s);
            }
            return ret;
        }

        public string GetPlanningFromType(string type)
        {
            if (!type.IsSet())
                return "Dwellinghouses";
            if (GetPlanningTypes().ContainsKey(type))
                return GetPlanningTypes()[type];
            return "Dwellinghouses";
        }
    }

}

